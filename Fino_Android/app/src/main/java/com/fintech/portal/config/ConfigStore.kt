package com.fintech.portal.config

import android.content.Context
import android.content.SharedPreferences
import com.fintech.portal.BuildConfig
import com.fintech.portal.util.Net
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import org.json.JSONObject

/**
 * Remote config ka single source of truth.
 *
 * - First run: the defaults baked into the APK are used.
 * - Every launch fetches app-config.json from the server (in the background, with a timeout).
 * - The last successful config is cached, so branding stays correct offline too.
 */
object ConfigStore {

    private const val PREFS = "portal_config"
    private const val KEY_JSON = "remote_json"
    private const val KEY_URL_OVERRIDE = "dev_url_override"
    private const val KEY_FETCHED_AT = "fetched_at"

    private lateinit var prefs: SharedPreferences

    @Volatile
    var current: AppConfig = AppConfig.defaults()
        private set

    fun init(context: Context) {
        prefs = context.applicationContext.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
        current = buildFromCache()
    }

    private fun buildFromCache(): AppConfig {
        val base = AppConfig.defaults()
        val cached = prefs.getString(KEY_JSON, null) ?: return applyDevOverride(base)
        val merged = runCatching { AppConfig.merge(base, JSONObject(cached)) }.getOrDefault(base)
        return applyDevOverride(merged)
    }

    /** Lets a tester point a debug build at their own URL (long-press the splash logo). */
    private fun applyDevOverride(config: AppConfig): AppConfig {
        if (!BuildConfig.DEV_TOOLS) return config
        val override = prefs.getString(KEY_URL_OVERRIDE, null)?.trim().orEmpty()
        if (override.isBlank()) return config
        val host = runCatching { java.net.URI(override).host }.getOrNull().orEmpty()
        return config.copy(
            portalUrl = override.trimEnd('/'),
            internalHosts = if (host.isBlank()) config.internalHosts else config.internalHosts + host
        )
    }

    var devUrlOverride: String
        get() = prefs.getString(KEY_URL_OVERRIDE, "").orEmpty()
        set(value) {
            prefs.edit().putString(KEY_URL_OVERRIDE, value.trim()).apply()
            current = buildFromCache()
        }

    val lastFetchedAt: Long get() = prefs.getLong(KEY_FETCHED_AT, 0L)

    /** Fetch the config. On failure, quietly keep running on the previous one. */
    suspend fun refresh(): Boolean = withContext(Dispatchers.IO) {
        val url = BuildConfig.REMOTE_CONFIG_URL
        if (url.isBlank()) return@withContext false

        val body = Net.get(url) ?: return@withContext false
        val json = runCatching { JSONObject(body) }.getOrNull() ?: return@withContext false

        prefs.edit()
            .putString(KEY_JSON, json.toString())
            .putLong(KEY_FETCHED_AT, System.currentTimeMillis())
            .apply()

        current = buildFromCache()
        true
    }
}
