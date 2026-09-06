package com.fintech.portal.util

import android.content.Context
import android.graphics.Color
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import android.os.Build
import android.widget.Toast

fun String?.toColorOr(fallback: Int): Int = try {
    if (this.isNullOrBlank()) fallback else Color.parseColor(this.trim())
} catch (_: IllegalArgumentException) {
    fallback
}

/** Background ke hisaab se readable text color (white ya dark). */
fun Int.onColor(): Int {
    val luminance = (0.299 * Color.red(this) + 0.587 * Color.green(this) + 0.114 * Color.blue(this)) / 255.0
    return if (luminance > 0.6) Color.parseColor("#0F172A") else Color.WHITE
}

fun Int.darken(factor: Float = 0.85f): Int = Color.rgb(
    (Color.red(this) * factor).toInt().coerceIn(0, 255),
    (Color.green(this) * factor).toInt().coerceIn(0, 255),
    (Color.blue(this) * factor).toInt().coerceIn(0, 255)
)

fun Context.isOnline(): Boolean {
    val cm = getSystemService(Context.CONNECTIVITY_SERVICE) as? ConnectivityManager ?: return true
    return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
        val caps = cm.getNetworkCapabilities(cm.activeNetwork) ?: return false
        caps.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
    } else {
        @Suppress("DEPRECATION")
        cm.activeNetworkInfo?.isConnected == true
    }
}

fun Context.toast(message: String) {
    Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
}
