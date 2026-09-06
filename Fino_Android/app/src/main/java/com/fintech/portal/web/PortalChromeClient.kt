package com.fintech.portal.web

import android.Manifest
import android.app.Activity
import android.content.pm.PackageManager
import android.net.Uri
import android.webkit.ConsoleMessage
import android.webkit.GeolocationPermissions
import android.webkit.PermissionRequest
import android.webkit.ValueCallback
import android.webkit.WebChromeClient
import android.webkit.WebView
import androidx.core.content.ContextCompat
import com.fintech.portal.BuildConfig

class PortalChromeClient(
    private val activity: Activity,
    private val fileChooser: FileChooser,
    private val onProgress: (Int) -> Unit,
    private val onTitle: (String) -> Unit,
    private val requestPermission: (String, (Boolean) -> Unit) -> Unit
) : WebChromeClient() {

    override fun onProgressChanged(view: WebView, newProgress: Int) = onProgress(newProgress)

    override fun onReceivedTitle(view: WebView, title: String?) {
        if (!title.isNullOrBlank()) onTitle(title)
    }

    /** KYC / fund-request image upload: camera ya gallery se file pick. */
    override fun onShowFileChooser(
        webView: WebView,
        filePathCallback: ValueCallback<Array<Uri>>,
        fileChooserParams: FileChooserParams
    ): Boolean = fileChooser.open(filePathCallback, fileChooserParams)

    /** Web se camera/mic maanga gaya (e.g. video KYC) — pehle Android permission lo. */
    override fun onPermissionRequest(request: PermissionRequest) {
        val wanted = request.resources.filter {
            it == PermissionRequest.RESOURCE_VIDEO_CAPTURE || it == PermissionRequest.RESOURCE_AUDIO_CAPTURE
        }
        if (wanted.isEmpty()) {
            request.deny()
            return
        }
        val androidPerm = if (wanted.contains(PermissionRequest.RESOURCE_VIDEO_CAPTURE))
            Manifest.permission.CAMERA else Manifest.permission.RECORD_AUDIO

        if (ContextCompat.checkSelfPermission(activity, androidPerm) == PackageManager.PERMISSION_GRANTED) {
            activity.runOnUiThread { request.grant(wanted.toTypedArray()) }
        } else {
            requestPermission(androidPerm) { granted ->
                activity.runOnUiThread {
                    if (granted) request.grant(wanted.toTypedArray()) else request.deny()
                }
            }
        }
    }

    /** For AEPS and other location-tagged transactions. */
    override fun onGeolocationPermissionsShowPrompt(
        origin: String,
        callback: GeolocationPermissions.Callback
    ) {
        val perm = Manifest.permission.ACCESS_FINE_LOCATION
        if (ContextCompat.checkSelfPermission(activity, perm) == PackageManager.PERMISSION_GRANTED) {
            callback.invoke(origin, true, false)
        } else {
            requestPermission(perm) { granted -> callback.invoke(origin, granted, false) }
        }
    }

    /**
     * target="_blank" / window.open() links.
     *
     * We hand back a throwaway WebView to capture the URL, then let the router decide:
     * our own portal stays in this WebView, anything else goes to a Custom Tab or another app.
     */
    override fun onCreateWindow(
        view: WebView,
        isDialog: Boolean,
        isUserGesture: Boolean,
        resultMsg: android.os.Message
    ): Boolean {
        val scout = WebView(activity)
        scout.webViewClient = object : android.webkit.WebViewClient() {
            override fun shouldOverrideUrlLoading(
                scoutView: WebView,
                request: android.webkit.WebResourceRequest
            ): Boolean {
                val url = request.url?.toString().orEmpty()
                val config = com.fintech.portal.config.ConfigStore.current
                if (!UrlRouter.handle(activity, url, config)) {
                    view.loadUrl(url)
                }
                scoutView.destroy()
                return true
            }
        }

        val transport = resultMsg.obj as? WebView.WebViewTransport
        if (transport == null) {
            scout.destroy()
            return false
        }
        transport.webView = scout
        resultMsg.sendToTarget()
        return true
    }

    override fun onConsoleMessage(message: ConsoleMessage): Boolean {
        if (BuildConfig.DEV_TOOLS) {
            android.util.Log.d(
                "PortalWeb",
                "${message.message()} @${message.sourceId()}:${message.lineNumber()}"
            )
        }
        return true
    }
}
