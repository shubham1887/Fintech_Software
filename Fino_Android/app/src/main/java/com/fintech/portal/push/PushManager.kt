package com.fintech.portal.push

import android.content.Context
import android.util.Log
import com.fintech.portal.BuildConfig
import com.fintech.portal.config.AppConfig
import com.google.firebase.FirebaseApp
import com.google.firebase.FirebaseOptions
import com.google.firebase.messaging.FirebaseMessaging

/**
 * Firebase, configured at RUN TIME rather than from a bundled `google-services.json`.
 *
 * Why that matters here: this platform is sold as a white-label install, one APK per client,
 * and a `google-services.json` is a build-time file — adopting the normal Gradle plugin would
 * mean a rebuild and a fresh Play upload every time a client's Firebase project changes.
 * Instead the four values live in the same hosted `app-config.json` that already carries the
 * portal URL and the branding, so a client's push is switched on by editing one file on their
 * own server.
 *
 * It initialises the DEFAULT FirebaseApp on purpose, not a named secondary one:
 * FirebaseMessaging only ever works against the default app — ask a named app for it and it
 * throws. With no google-services.json in the APK, Firebase's own init provider leaves the
 * default slot empty at startup (it logs a warning and carries on), which is exactly the slot
 * this fills.
 *
 * With nothing configured, every call here is a quiet no-op: the app works as before and
 * alerts still reach the user through the in-app bell.
 */
object PushManager {

    private const val TAG = "PushManager"

    @Volatile
    private var ready = false

    /** The last token Firebase gave us, so a late-loading WebView can still be told. */
    @Volatile
    var currentToken: String? = null
        private set

    /** True once a Firebase project is configured and initialised. */
    val isReady: Boolean get() = ready

    /**
     * Initialise Firebase from the current config. Safe to call on every launch and again
     * after the remote config refreshes — the first success wins.
     */
    @Synchronized
    fun init(context: Context, config: AppConfig) {
        if (ready) return
        if (!config.pushConfigured()) return

        val app = context.applicationContext

        // Already initialised (a build that DID ship google-services.json, or a second call
        // after a config refresh) — take it rather than throwing.
        val existing = runCatching { FirebaseApp.getInstance() }.getOrNull()
        if (existing != null) {
            ready = true
            return
        }

        ready = try {
            FirebaseApp.initializeApp(
                app,
                FirebaseOptions.Builder()
                    .setProjectId(config.fcmProjectId)
                    .setApplicationId(config.fcmAppId)
                    .setApiKey(config.fcmApiKey)
                    .setGcmSenderId(config.fcmSenderId)
                    .build(),
            ) != null
        } catch (t: Throwable) {
            // Bad values in app-config.json must never stop the app from opening.
            if (BuildConfig.DEV_TOOLS) Log.w(TAG, "Firebase init failed", t)
            false
        }

        if (ready) PushChannels.ensure(app, config.brandName)
    }

    /**
     * Ask Firebase for this device's token. [onToken] runs when it arrives, and not at all
     * when push is unconfigured or the fetch fails.
     */
    fun fetchToken(onToken: (String) -> Unit) {
        if (!ready) return

        // The token may already be in hand from onNewToken — hand it over without a round trip.
        currentToken?.let { onToken(it); return }

        try {
            FirebaseMessaging.getInstance().token
                .addOnSuccessListener { token ->
                    if (!token.isNullOrBlank()) {
                        currentToken = token
                        onToken(token)
                    }
                }
                .addOnFailureListener { t ->
                    if (BuildConfig.DEV_TOOLS) Log.w(TAG, "Token fetch failed", t)
                }
        } catch (t: Throwable) {
            if (BuildConfig.DEV_TOOLS) Log.w(TAG, "Token fetch threw", t)
        }
    }

    /** Called by the messaging service when Firebase rotates the token. */
    fun rememberToken(token: String) {
        currentToken = token
    }

    /**
     * Forget the token on this device. Called when the shell clears the session, so the next
     * person to sign in on a shared counter phone does not inherit the previous user's alerts.
     * The API-side unregister is done by the web layer while its session is still valid.
     */
    fun clearToken() {
        currentToken = null
        if (!ready) return
        try {
            // deleteToken() is a network call; it must not run on the main thread's critical
            // path, but the Task API already takes it off-thread for us.
            FirebaseMessaging.getInstance().deleteToken()
        } catch (t: Throwable) {
            if (BuildConfig.DEV_TOOLS) Log.w(TAG, "Token delete failed", t)
        }
    }
}
