package com.fintech.portal.web

import android.app.Activity
import android.content.ActivityNotFoundException
import android.content.Intent
import android.graphics.Color
import android.net.Uri
import androidx.browser.customtabs.CustomTabsIntent
import com.fintech.portal.config.AppConfig
import com.fintech.portal.util.toColorOr
import com.fintech.portal.util.toast

/**
 * Har URL ka faisla: app ke andar khole, Chrome Custom Tab me khole, ya kisi
 * dusri app ko de de (dialer, WhatsApp, UPI app, Play Store...).
 */
object UrlRouter {

    private val APP_SCHEMES = setOf(
        "tel", "sms", "smsto", "mailto", "geo", "upi", "market",
        "whatsapp", "tg", "intent", "bharatpe", "phonepe", "paytmmp", "gpay"
    )

    /** true = handled here; the WebView must not load it. */
    fun handle(activity: Activity, rawUrl: String, config: AppConfig): Boolean {
        val uri = runCatching { Uri.parse(rawUrl) }.getOrNull() ?: return false
        val scheme = uri.scheme?.lowercase().orEmpty()

        return when {
            scheme == "http" || scheme == "https" -> {
                if (config.isInternalHost(uri.host)) {
                    false // our own portal — let it stay in the WebView
                } else {
                    openCustomTab(activity, rawUrl, config)
                    true
                }
            }

            scheme == "blob" || scheme == "data" || scheme == "about" || scheme == "javascript" ->
                false // download listener / WebView khud sambhalega

            scheme == "intent" -> handleIntentScheme(activity, rawUrl)

            scheme in APP_SCHEMES -> openExternalApp(activity, rawUrl)

            scheme.isBlank() -> false

            else -> openExternalApp(activity, rawUrl)
        }
    }

    /** External web links open in a branded in-app Chrome Custom Tab, not the browser. */
    fun openCustomTab(activity: Activity, url: String, config: AppConfig) {
        val toolbar = config.colorPrimary.toColorOr(Color.parseColor("#1D4ED8"))
        try {
            CustomTabsIntent.Builder()
                .setShowTitle(true)
                .setUrlBarHidingEnabled(true)
                .setDefaultColorSchemeParams(
                    androidx.browser.customtabs.CustomTabColorSchemeParams.Builder()
                        .setToolbarColor(toolbar)
                        .build()
                )
                .build()
                .launchUrl(activity, Uri.parse(url))
        } catch (_: Throwable) {
            openInBrowser(activity, url)
        }
    }

    private fun handleIntentScheme(activity: Activity, url: String): Boolean {
        return try {
            val intent = Intent.parseUri(url, Intent.URI_INTENT_SCHEME)
            try {
                activity.startActivity(intent)
                true
            } catch (_: ActivityNotFoundException) {
                val fallback = intent.getStringExtra("browser_fallback_url")
                if (!fallback.isNullOrBlank()) {
                    openInBrowser(activity, fallback)
                } else {
                    val pkg = intent.`package`
                    if (!pkg.isNullOrBlank()) {
                        openInBrowser(activity, "https://play.google.com/store/apps/details?id=$pkg")
                    } else {
                        activity.toast("No app found to open this link")
                    }
                }
                true
            }
        } catch (_: Throwable) {
            false
        }
    }

    private fun openExternalApp(activity: Activity, url: String): Boolean = try {
        activity.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(url)).apply {
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        })
        true
    } catch (_: ActivityNotFoundException) {
        activity.toast("No app installed for this link")
        true
    } catch (_: Throwable) {
        false
    }

    fun openInBrowser(activity: Activity, url: String) {
        try {
            activity.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(url)).apply {
                addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            })
        } catch (_: Throwable) {
            activity.toast("Could not open the link")
        }
    }

    fun openWhatsApp(activity: Activity, number: String, text: String) {
        val digits = number.filter { it.isDigit() }
        val url = "https://wa.me/$digits" + if (text.isBlank()) "" else "?text=" + Uri.encode(text)
        openInBrowser(activity, url)
    }
}
