package com.fintech.portal.util

import android.util.Log
import java.net.HttpURLConnection
import java.net.URL

/** A minimal HTTP GET — used only for the remote config. No extra dependency. */
object Net {

    private const val TAG = "Net"

    fun get(url: String, timeoutMs: Int = 6000): String? {
        var conn: HttpURLConnection? = null
        return try {
            conn = (URL(url).openConnection() as HttpURLConnection).apply {
                requestMethod = "GET"
                connectTimeout = timeoutMs
                readTimeout = timeoutMs
                instanceFollowRedirects = true
                setRequestProperty("Accept", "application/json")
                setRequestProperty("Cache-Control", "no-cache")
            }
            if (conn.responseCode in 200..299) {
                conn.inputStream.bufferedReader().use { it.readText() }
            } else {
                Log.w(TAG, "config fetch HTTP ${conn.responseCode}")
                null
            }
        } catch (t: Throwable) {
            Log.w(TAG, "config fetch failed: ${t.message}")
            null
        } finally {
            runCatching { conn?.disconnect() }
        }
    }
}
