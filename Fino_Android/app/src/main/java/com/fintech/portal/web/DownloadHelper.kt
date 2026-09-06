package com.fintech.portal.web

import android.app.Activity
import android.app.DownloadManager
import android.content.ContentValues
import android.content.Context
import android.net.Uri
import android.os.Build
import android.os.Environment
import android.provider.MediaStore
import android.util.Base64
import android.webkit.CookieManager
import android.webkit.URLUtil
import android.webkit.WebView
import com.fintech.portal.util.toast
import java.io.File
import java.io.FileOutputStream

/**
 * Reports / receipts / statements download.
 *
 * Three kinds of URL arrive here:
 *  - normal http(s)  -> Android DownloadManager (notification ke saath)
 *  - blob:           -> read it as base64 in JS, then save natively (how SPA exports arrive)
 *  - data:           -> seedha decode karke save
 */
object DownloadHelper {

    fun handle(
        activity: Activity,
        webView: WebView,
        url: String,
        userAgent: String?,
        contentDisposition: String?,
        mimeType: String?
    ) {
        when {
            url.startsWith("blob:", true) -> webView.evaluateJavascript(blobToBase64(url, mimeType), null)
            url.startsWith("data:", true) -> saveDataUri(activity, url)
            else -> enqueue(activity, url, userAgent, contentDisposition, mimeType)
        }
    }

    private fun enqueue(
        activity: Activity,
        url: String,
        userAgent: String?,
        contentDisposition: String?,
        mimeType: String?
    ) {
        try {
            val fileName = URLUtil.guessFileName(url, contentDisposition, mimeType)
            val request = DownloadManager.Request(Uri.parse(url)).apply {
                setMimeType(mimeType)
                addRequestHeader("cookie", CookieManager.getInstance().getCookie(url))
                if (!userAgent.isNullOrBlank()) addRequestHeader("User-Agent", userAgent)
                setTitle(fileName)
                setDescription("Downloading…")
                setNotificationVisibility(DownloadManager.Request.VISIBILITY_VISIBLE_NOTIFY_COMPLETED)
                setDestinationInExternalPublicDir(Environment.DIRECTORY_DOWNLOADS, fileName)
                allowScanningByMediaScanner()
            }
            val dm = activity.getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
            dm.enqueue(request)
            activity.toast("Download started: $fileName")
        } catch (t: Throwable) {
            activity.toast("Download failed: ${t.message}")
        }
    }

    private fun saveDataUri(activity: Activity, url: String) {
        try {
            val meta = url.substringAfter("data:").substringBefore(",")
            val mime = meta.substringBefore(";").ifBlank { "application/octet-stream" }
            val payload = url.substringAfter(",")
            val bytes = if (meta.contains("base64", true)) {
                Base64.decode(payload, Base64.DEFAULT)
            } else {
                Uri.decode(payload).toByteArray()
            }
            val name = "download_${System.currentTimeMillis()}${extensionFor(mime)}"
            saveBytes(activity, name, mime, bytes)
        } catch (t: Throwable) {
            activity.toast("Could not save file: ${t.message}")
        }
    }

    /**
     * Converts a blob: URL to base64 inside the page and hands it to the native bridge.
     * (A WebView cannot download blob URLs on its own.)
     */
    private fun blobToBase64(blobUrl: String, mimeType: String?): String = """
        (function () {
          try {
            var xhr = new XMLHttpRequest();
            xhr.open('GET', '$blobUrl', true);
            xhr.responseType = 'blob';
            xhr.onload = function () {
              if (xhr.status !== 200) return;
              var reader = new FileReader();
              reader.onloadend = function () {
                var base64 = String(reader.result).split(',')[1] || '';
                var mime = xhr.response.type || '${mimeType ?: "application/octet-stream"}';
                if (window.AndroidBridge && window.AndroidBridge.saveBase64File) {
                  window.AndroidBridge.saveBase64File('', mime, base64);
                }
              };
              reader.readAsDataURL(xhr.response);
            };
            xhr.send();
          } catch (e) { }
        })();
    """.trimIndent()

    fun saveBytes(context: Context, fileName: String, mime: String, bytes: ByteArray) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            val values = ContentValues().apply {
                put(MediaStore.Downloads.DISPLAY_NAME, fileName)
                put(MediaStore.Downloads.MIME_TYPE, mime)
                put(MediaStore.Downloads.IS_PENDING, 1)
            }
            val resolver = context.contentResolver
            val uri = resolver.insert(MediaStore.Downloads.EXTERNAL_CONTENT_URI, values)
                ?: throw IllegalStateException("Could not create the file in Downloads")
            resolver.openOutputStream(uri)?.use { it.write(bytes) }
            values.clear()
            values.put(MediaStore.Downloads.IS_PENDING, 0)
            resolver.update(uri, values, null, null)
        } else {
            @Suppress("DEPRECATION")
            val dir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS)
            dir.mkdirs()
            FileOutputStream(File(dir, fileName)).use { it.write(bytes) }
        }
    }

    fun extensionFor(mime: String): String = when {
        mime.contains("pdf") -> ".pdf"
        mime.contains("sheet") || mime.contains("excel") -> ".xlsx"
        mime.contains("csv") -> ".csv"
        mime.contains("png") -> ".png"
        mime.contains("jpeg") || mime.contains("jpg") -> ".jpg"
        mime.contains("json") -> ".json"
        mime.contains("plain") -> ".txt"
        else -> ".bin"
    }
}
