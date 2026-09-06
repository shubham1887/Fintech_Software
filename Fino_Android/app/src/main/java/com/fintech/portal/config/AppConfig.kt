package com.fintech.portal.config

import com.fintech.portal.BuildConfig
import org.json.JSONArray
import org.json.JSONObject

/**
 * Everything about how the app behaves is controlled from here.
 *
 * Do layer:
 *  1) BUILD-TIME  -> whitelabel/clients.json (one APK per client: name, colours, URL)
 *  2) RUNTIME     -> app-config.json hosted on the server (change anything without a new APK)
 *
 * Runtime values override the build-time defaults.
 */
data class AppConfig(
    val portalUrl: String,
    /** App khulte hi kaunsa page — default "/login". */
    val startPath: String,
    val brandName: String,
    val colorPrimary: String,
    val colorAccent: String,
    val colorSplash: String,

    /** These hosts stay inside the app; everything else opens outside it. */
    val internalHosts: List<String>,

    val pullToRefresh: Boolean,
    val blockScreenshots: Boolean,
    val allowFileUpload: Boolean,
    val allowDownloads: Boolean,
    val confirmExit: Boolean,
    val userAgentSuffix: String,

    /** Force update */
    val minVersionCode: Int,
    val updateUrl: String,
    val updateMessage: String,
    val forceUpdate: Boolean,

    /** Maintenance */
    val maintenance: Boolean,
    val maintenanceMessage: String,

    /** Support */
    val supportWhatsApp: String,
    val supportEmail: String,
    val supportPhone: String
) {

    companion object {

        /** APK ke andar bake hui default config (offline / pehli baar). */
        fun defaults(): AppConfig {
            val host = BuildConfig.PORTAL_HOST
            return AppConfig(
                portalUrl = BuildConfig.PORTAL_URL,
                startPath = BuildConfig.START_PATH,
                brandName = BuildConfig.BRAND_NAME,
                colorPrimary = BuildConfig.COLOR_PRIMARY,
                colorAccent = BuildConfig.COLOR_ACCENT,
                colorSplash = BuildConfig.COLOR_SPLASH,
                internalHosts = if (host.isBlank()) emptyList() else listOf(host),
                pullToRefresh = BuildConfig.PULL_TO_REFRESH,
                blockScreenshots = BuildConfig.BLOCK_SCREENSHOTS,
                allowFileUpload = true,
                allowDownloads = true,
                confirmExit = true,
                userAgentSuffix = "${BuildConfig.CLIENT_ID}App/${BuildConfig.VERSION_NAME}",
                minVersionCode = 0,
                updateUrl = "https://play.google.com/store/apps/details?id=${BuildConfig.APPLICATION_ID}",
                updateMessage = "",
                forceUpdate = false,
                maintenance = false,
                maintenanceMessage = "",
                supportWhatsApp = BuildConfig.SUPPORT_WHATSAPP,
                supportEmail = BuildConfig.SUPPORT_EMAIL,
                supportPhone = BuildConfig.SUPPORT_PHONE
            )
        }

        /** Merge the server's JSON over the defaults. A missing key keeps the default. */
        fun merge(base: AppConfig, json: JSONObject): AppConfig {
            fun str(key: String, fallback: String) =
                json.optString(key, "").ifBlank { fallback }

            fun bool(key: String, fallback: Boolean) =
                if (json.has(key)) json.optBoolean(key, fallback) else fallback

            fun int(key: String, fallback: Int) =
                if (json.has(key)) json.optInt(key, fallback) else fallback

            fun list(key: String, fallback: List<String>): List<String> {
                val arr: JSONArray = json.optJSONArray(key) ?: return fallback
                val out = ArrayList<String>(arr.length())
                for (i in 0 until arr.length()) {
                    val v = arr.optString(i, "").trim()
                    if (v.isNotEmpty()) out.add(v)
                }
                return if (out.isEmpty()) fallback else out
            }

            val portal = str("portalUrl", base.portalUrl).trimEnd('/')
            val portalHost = runCatching { java.net.URI(portal).host }.getOrNull().orEmpty()
            val hosts = list("internalHosts", base.internalHosts)
                .let { if (portalHost.isNotBlank() && !it.contains(portalHost)) it + portalHost else it }

            return base.copy(
                portalUrl = portal,
                startPath = str("startPath", base.startPath),
                brandName = str("brandName", base.brandName),
                colorPrimary = str("colorPrimary", base.colorPrimary),
                colorAccent = str("colorAccent", base.colorAccent),
                colorSplash = str("colorSplash", base.colorSplash),
                internalHosts = hosts,
                pullToRefresh = bool("pullToRefresh", base.pullToRefresh),
                blockScreenshots = bool("blockScreenshots", base.blockScreenshots),
                allowFileUpload = bool("allowFileUpload", base.allowFileUpload),
                allowDownloads = bool("allowDownloads", base.allowDownloads),
                confirmExit = bool("confirmExit", base.confirmExit),
                userAgentSuffix = str("userAgentSuffix", base.userAgentSuffix),
                minVersionCode = int("minVersionCode", base.minVersionCode),
                updateUrl = str("updateUrl", base.updateUrl),
                updateMessage = str("updateMessage", base.updateMessage),
                forceUpdate = bool("forceUpdate", base.forceUpdate),
                maintenance = bool("maintenance", base.maintenance),
                maintenanceMessage = str("maintenanceMessage", base.maintenanceMessage),
                supportWhatsApp = str("supportWhatsApp", base.supportWhatsApp),
                supportEmail = str("supportEmail", base.supportEmail),
                supportPhone = str("supportPhone", base.supportPhone)
            )
        }
    }

    /** The URL opened on app launch. */
    fun startUrl(): String {
        val base = portalUrl.trimEnd('/')
        val path = startPath.trim()
        return when {
            path.isBlank() || path == "/" -> base
            path.startsWith("http", true) -> path
            path.startsWith("/") -> base + path
            else -> "$base/$path"
        }
    }

    /** Should this URL open inside the app? */
    fun isInternalHost(host: String?): Boolean {
        if (host.isNullOrBlank()) return false
        val h = host.lowercase()
        return internalHosts.any { allowed ->
            val a = allowed.lowercase().removePrefix("*.")
            h == a || h.endsWith(".$a")
        }
    }
}
