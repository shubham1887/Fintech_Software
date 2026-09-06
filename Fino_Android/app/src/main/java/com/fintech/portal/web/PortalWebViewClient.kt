package com.fintech.portal.web

import android.app.Activity
import android.graphics.Bitmap
import android.net.http.SslError
import android.webkit.SslErrorHandler
import android.webkit.WebResourceError
import android.webkit.WebResourceRequest
import android.webkit.WebResourceResponse
import android.webkit.WebView
import android.webkit.WebViewClient
import com.fintech.portal.config.ConfigStore
import com.fintech.portal.util.JsBridgeScript

class PortalWebViewClient(
    private val activity: Activity,
    private val onStart: (String) -> Unit,
    private val onFinish: (String) -> Unit,
    private val onError: (String) -> Unit
) : WebViewClient() {

    override fun shouldOverrideUrlLoading(view: WebView, request: WebResourceRequest): Boolean {
        val url = request.url?.toString().orEmpty()
        return UrlRouter.handle(activity, url, ConfigStore.current)
    }

    override fun onPageStarted(view: WebView, url: String, favicon: Bitmap?) {
        super.onPageStarted(view, url, favicon)
        onStart(url)
    }

    override fun onPageFinished(view: WebView, url: String) {
        super.onPageFinished(view, url)
        // Inject the native bridge's JS shim on every page
        view.evaluateJavascript(JsBridgeScript.build(), null)
        onFinish(url)
    }

    override fun onReceivedError(
        view: WebView,
        request: WebResourceRequest,
        error: WebResourceError
    ) {
        super.onReceivedError(view, request, error)
        // Only a main-document failure matters; ignore images/ads
        if (request.isForMainFrame) {
            onError(error.description?.toString() ?: "Connection error")
        }
    }

    override fun onReceivedHttpError(
        view: WebView,
        request: WebResourceRequest,
        errorResponse: WebResourceResponse
    ) {
        super.onReceivedHttpError(view, request, errorResponse)
        if (request.isForMainFrame && errorResponse.statusCode >= 500) {
            onError("Server error " + errorResponse.statusCode)
        }
    }

    /**
     * This is a payments app — never proceed past an invalid/self-signed certificate.
     * (If dev needs a self-signed cert, allow it in the debug build only, never in release.)
     */
    override fun onReceivedSslError(view: WebView, handler: SslErrorHandler, error: SslError) {
        handler.cancel()
        onError("Secure connection failed (invalid SSL certificate)")
    }
}
