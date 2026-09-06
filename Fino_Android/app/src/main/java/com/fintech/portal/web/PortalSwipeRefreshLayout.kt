package com.fintech.portal.web

import android.content.Context
import android.util.AttributeSet
import android.webkit.WebView
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout

/**
 * The scroll-jank fix.
 *
 * A plain SwipeRefreshLayout wrapped around a WebView makes scrolling stick. Why:
 * SwipeRefreshLayout reads `webView.scrollY` to decide whether the page is at the top —
 * but in a React SPA the real scroll usually belongs to an inner `overflow-y: auto` div,
 * so the WebView's own scrollY stays 0 forever. The layout therefore believes the page is
 * at the top and swallows every swipe as a pull-to-refresh.
 *
 * So we ask the page instead: JsBridgeScript reports, on scroll and touch, whether the
 * element actually being scrolled is at its top.
 */
class PortalSwipeRefreshLayout @JvmOverloads constructor(
    context: Context,
    attrs: AttributeSet? = null
) : SwipeRefreshLayout(context, attrs) {

    var webView: WebView? = null

    /** Reported by JS: is the element the user is touching scrolled to its top? */
    @Volatile
    var pageAtTop: Boolean = true

    override fun canChildScrollUp(): Boolean {
        val wv = webView ?: return super.canChildScrollUp()
        return wv.scrollY > 0 || !pageAtTop
    }
}
