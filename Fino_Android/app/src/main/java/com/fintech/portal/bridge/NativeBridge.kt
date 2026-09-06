package com.fintech.portal.bridge

import android.Manifest
import android.content.ClipData
import android.content.ClipboardManager
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.print.PrintAttributes
import android.print.PrintManager
import android.util.Base64
import android.view.WindowManager
import android.webkit.JavascriptInterface
import android.webkit.WebView
import androidx.appcompat.app.AppCompatActivity
import androidx.biometric.BiometricManager
import androidx.biometric.BiometricPrompt
import androidx.core.content.ContextCompat
import com.fintech.portal.BuildConfig
import com.fintech.portal.config.ConfigStore
import com.fintech.portal.util.SecureStore
import com.fintech.portal.util.toColorOr
import com.fintech.portal.util.toast
import com.fintech.portal.web.DownloadHelper
import com.fintech.portal.web.UrlRouter
import org.json.JSONObject

/**
 * The web <-> native bridge. The site sees it as window.AndroidBridge (and JsBridgeScript
 * injects the friendlier promise wrapper, window.NativeApp).
 *
 * NOTE: every method here runs on the WebView's JS thread — anything that touches the UI
 * must go through runOnUiThread.
 */
class NativeBridge(
    private val activity: AppCompatActivity,
    private val webView: WebView,
    private val requestPermission: (String, (Boolean) -> Unit) -> Unit,
    private val onLogout: () -> Unit,
    private val onScrollAtTop: (Boolean) -> Unit,
    private val onContentReady: () -> Unit
) {

    // ---------------------------------------------------------------- info

    @JavascriptInterface
    fun getAppInfo(): String = JSONObject().apply {
        put("platform", "android")
        put("isNativeApp", true)
        put("client", BuildConfig.CLIENT_ID)
        put("brand", ConfigStore.current.brandName)
        put("packageName", BuildConfig.APPLICATION_ID)
        put("versionName", BuildConfig.VERSION_NAME)
        put("versionCode", BuildConfig.VERSION_CODE)
        put("androidVersion", Build.VERSION.RELEASE)
        put("sdkInt", Build.VERSION.SDK_INT)
        put("device", Build.MANUFACTURER + " " + Build.MODEL)
    }.toString()

    @JavascriptInterface
    fun getConfig(): String = ConfigStore.current.let { c ->
        JSONObject().apply {
            put("portalUrl", c.portalUrl)
            put("brandName", c.brandName)
            put("colorPrimary", c.colorPrimary)
            put("colorAccent", c.colorAccent)
            put("supportWhatsApp", c.supportWhatsApp)
            put("supportEmail", c.supportEmail)
            put("supportPhone", c.supportPhone)
        }.toString()
    }

    // ------------------------------------------------------------------ ui

    @JavascriptInterface
    fun toast(message: String) = activity.runOnUiThread { activity.toast(message) }

    @JavascriptInterface
    fun haptic(type: String) {
        val vibrator = ContextCompat.getSystemService(activity, Vibrator::class.java) ?: return
        val ms = when (type.lowercase()) {
            "heavy" -> 60L
            "medium" -> 35L
            "success" -> 25L
            else -> 15L
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            vibrator.vibrate(VibrationEffect.createOneShot(ms, VibrationEffect.DEFAULT_AMPLITUDE))
        } else {
            @Suppress("DEPRECATION") vibrator.vibrate(ms)
        }
    }

    @JavascriptInterface
    fun setStatusBarColor(hex: String) = activity.runOnUiThread {
        @Suppress("DEPRECATION")
        activity.window.statusBarColor = hex.toColorOr(activity.window.statusBarColor)
    }

    /** Block screenshots and screen recording (for screens showing money). */
    @JavascriptInterface
    fun setSecureScreen(secure: Boolean) = activity.runOnUiThread {
        if (secure) {
            activity.window.setFlags(
                WindowManager.LayoutParams.FLAG_SECURE,
                WindowManager.LayoutParams.FLAG_SECURE
            )
        } else {
            activity.window.clearFlags(WindowManager.LayoutParams.FLAG_SECURE)
        }
    }

    // -------------------------------------------------------------- actions

    @JavascriptInterface
    fun share(title: String, text: String) = activity.runOnUiThread {
        val intent = Intent(Intent.ACTION_SEND).apply {
            type = "text/plain"
            putExtra(Intent.EXTRA_SUBJECT, title)
            putExtra(Intent.EXTRA_TEXT, text)
        }
        activity.startActivity(Intent.createChooser(intent, title.ifBlank { "Share" }))
    }

    @JavascriptInterface
    fun copyToClipboard(label: String, text: String) = activity.runOnUiThread {
        val cm = activity.getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
        cm.setPrimaryClip(ClipData.newPlainText(label.ifBlank { "copied" }, text))
        activity.toast("Copied")
    }

    @JavascriptInterface
    fun openExternal(url: String) = activity.runOnUiThread {
        UrlRouter.openCustomTab(activity, url, ConfigStore.current)
    }

    @JavascriptInterface
    fun openWhatsApp(number: String, message: String) = activity.runOnUiThread {
        val target = number.ifBlank { ConfigStore.current.supportWhatsApp }
        if (target.isBlank()) activity.toast("Support number is not set")
        else UrlRouter.openWhatsApp(activity, target, message)
    }

    @JavascriptInterface
    fun call(number: String) = activity.runOnUiThread {
        val target = number.ifBlank { ConfigStore.current.supportPhone }
        if (target.isBlank()) {
            activity.toast("Support number is not set")
        } else {
            activity.startActivity(Intent(Intent.ACTION_DIAL, Uri.parse("tel:" + target)))
        }
    }

    /** Receipt / statement print ya PDF me save. */
    @JavascriptInterface
    fun printPage(documentName: String) = activity.runOnUiThread {
        try {
            val name = documentName.ifBlank { ConfigStore.current.brandName + " receipt" }
            val printManager = activity.getSystemService(Context.PRINT_SERVICE) as PrintManager
            printManager.print(
                name,
                webView.createPrintDocumentAdapter(name),
                PrintAttributes.Builder().build()
            )
        } catch (t: Throwable) {
            activity.toast("Could not print: " + t.message)
        }
    }

    /** Save a blob/base64 file into the Downloads folder. */
    @JavascriptInterface
    fun saveBase64File(fileName: String, mimeType: String, base64: String) {
        try {
            val mime = mimeType.ifBlank { "application/octet-stream" }
            val name = fileName.ifBlank {
                BuildConfig.CLIENT_ID + "_" + System.currentTimeMillis() + DownloadHelper.extensionFor(mime)
            }
            val bytes = Base64.decode(base64, Base64.DEFAULT)
            DownloadHelper.saveBytes(activity, name, mime, bytes)
            activity.runOnUiThread { activity.toast("Saved to Downloads: " + name) }
        } catch (t: Throwable) {
            activity.runOnUiThread { activity.toast("Could not save: " + t.message) }
        }
    }

    @JavascriptInterface
    fun requestNotificationPermission() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            activity.runOnUiThread {
                requestPermission(Manifest.permission.POST_NOTIFICATIONS) { granted ->
                    emit("notificationPermission", JSONObject().put("granted", granted))
                }
            }
        } else {
            emit("notificationPermission", JSONObject().put("granted", true))
        }
    }

    // ----------------------------------------------------------- biometric

    @JavascriptInterface
    fun biometricAvailable(): Boolean {
        val status = BiometricManager.from(activity).canAuthenticate(
            BiometricManager.Authenticators.BIOMETRIC_WEAK or
                BiometricManager.Authenticators.DEVICE_CREDENTIAL
        )
        return status == BiometricManager.BIOMETRIC_SUCCESS
    }

    /**
     * Unlock with fingerprint, face or device PIN. The result arrives as a JS event:
     *   NativeApp.on('biometric', function (e) { ... })
     */
    @JavascriptInterface
    fun biometricAuth(title: String, subtitle: String) = activity.runOnUiThread {
        if (!biometricAvailable()) {
            emit("biometric", JSONObject().put("ok", false).put("reason", "unavailable"))
            return@runOnUiThread
        }

        val prompt = BiometricPrompt(
            activity,
            ContextCompat.getMainExecutor(activity),
            object : BiometricPrompt.AuthenticationCallback() {
                override fun onAuthenticationSucceeded(result: BiometricPrompt.AuthenticationResult) {
                    emit("biometric", JSONObject().put("ok", true))
                }

                override fun onAuthenticationError(code: Int, message: CharSequence) {
                    emit("biometric", JSONObject().put("ok", false).put("reason", message.toString()))
                }
            }
        )

        prompt.authenticate(
            BiometricPrompt.PromptInfo.Builder()
                .setTitle(title.ifBlank { ConfigStore.current.brandName + " unlock" })
                .setSubtitle(subtitle.ifBlank { "Confirm your identity" })
                .setAllowedAuthenticators(
                    BiometricManager.Authenticators.BIOMETRIC_WEAK or
                        BiometricManager.Authenticators.DEVICE_CREDENTIAL
                )
                .build()
        )
    }

    // ------------------------------------------------------- secure store

    @JavascriptInterface
    fun secureSet(key: String, value: String): Boolean = SecureStore.put(activity, key, value)

    @JavascriptInterface
    fun secureGet(key: String): String = SecureStore.get(activity, key).orEmpty()

    @JavascriptInterface
    fun secureRemove(key: String) = SecureStore.remove(activity, key)

    // -------------------------------------------------------------- scroll

    /**
     * The page reports whether the element currently being scrolled is at its top.
     * That is what decides pull-to-refresh — without it, swipes stick inside the SPA's
     * own scroll containers.
     *
     * Called from the JS thread, so this only sets a volatile flag.
     */
    @JavascriptInterface
    fun setScrollAtTop(atTop: Boolean) = onScrollAtTop(atTop)

    /** The site can say "my content is ready" -> drop the branding overlay. */
    @JavascriptInterface
    fun contentReady() = activity.runOnUiThread { onContentReady() }

    // ----------------------------------------------------------- lifecycle

    @JavascriptInterface
    fun reloadApp() = activity.runOnUiThread { webView.reload() }

    @JavascriptInterface
    fun clearSession() = activity.runOnUiThread { onLogout() }

    @JavascriptInterface
    fun exitApp() = activity.runOnUiThread { activity.finishAffinity() }

    // --------------------------------------------------------------- utils

    /** Native -> JS event. Website: window.addEventListener("native:biometric", ...) */
    private fun emit(event: String, payload: JSONObject) {
        val js = "window.dispatchEvent(new CustomEvent('native:" + event + "',{detail:" + payload + "}));"
        activity.runOnUiThread { webView.evaluateJavascript(js, null) }
    }
}
