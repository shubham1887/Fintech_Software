package com.fintech.portal.push

import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.graphics.Color
import android.util.Log
import androidx.core.app.NotificationCompat
import androidx.core.content.ContextCompat
import com.fintech.portal.BuildConfig
import com.fintech.portal.R
import com.fintech.portal.config.ConfigStore
import com.fintech.portal.ui.MainActivity
import com.fintech.portal.util.toColorOr
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage
import java.io.ByteArrayOutputStream
import java.io.InputStream
import java.net.HttpURLConnection
import java.net.URL
import kotlin.math.absoluteValue

/**
 * Receives pushes and puts them in the tray.
 *
 * The server sends BOTH a `notification` block and a `data` block. Android only auto-posts
 * the notification block while the app is in the background, and it ignores our channel
 * choice in some OEM skins, so this service builds the tray entry itself from `data` in every
 * case. That also means one code path to reason about instead of "it works when the app is
 * closed but not when it is open".
 *
 * Tapping opens MainActivity with the alert's in-app path attached; MainActivity forwards it
 * to the WebView, which navigates and refreshes the bell.
 */
class PortalMessagingService : FirebaseMessagingService() {

    override fun onNewToken(token: String) {
        super.onNewToken(token)
        // Remember it; the WebView registers it with the API the next time the console loads
        // (or immediately, if it is already open — MainActivity re-emits on resume).
        PushManager.rememberToken(token)
    }

    override fun onMessageReceived(message: RemoteMessage) {
        super.onMessageReceived(message)

        val data = message.data
        val title = data["title"]?.takeIf { it.isNotBlank() }
            ?: message.notification?.title
            ?: ConfigStore.current.brandName
        val body = data["body"]?.takeIf { it.isNotBlank() }
            ?: message.notification?.body
            ?: return // nothing to show — don't post an empty notification

        val url = data["url"].orEmpty()
        val notificationId = data["notificationId"]?.toIntOrNull() ?: 0
        val channelId = PushChannels.sanitise(
            message.notification?.channelId ?: channelForKind(data["kind"])
        )
        val image = data["image"]?.takeIf { it.isNotBlank() }
            ?: message.notification?.imageUrl?.toString()

        show(title, body, url, notificationId, channelId, image)
    }

    /** Mirrors the server's NotificationKind.ChannelFor, for data-only messages. */
    private fun channelForKind(kind: String?): String = when (kind?.lowercase()) {
        "promo" -> PushChannels.PROMOTIONS
        "txn", "fund" -> PushChannels.TRANSACTIONS
        else -> PushChannels.ALERTS
    }

    private fun show(
        title: String,
        body: String,
        url: String,
        notificationId: Int,
        channelId: String,
        imageUrl: String?,
    ) {
        val config = ConfigStore.current
        PushChannels.ensure(this, config.brandName)

        val intent = Intent(this, MainActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_SINGLE_TOP or Intent.FLAG_ACTIVITY_CLEAR_TOP
            if (url.isNotBlank()) putExtra(EXTRA_PUSH_URL, url)
            if (notificationId > 0) putExtra(EXTRA_PUSH_ID, notificationId)
        }

        // Distinct request code per alert, or every notification would reuse — and overwrite —
        // the first one's intent, sending every tap to the same screen.
        val requestCode = if (notificationId > 0) notificationId else System.currentTimeMillis().toInt().absoluteValue

        val pending = PendingIntent.getActivity(
            this,
            requestCode,
            intent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE,
        )

        val builder = NotificationCompat.Builder(this, channelId)
            .setSmallIcon(R.drawable.ic_notification)
            .setColor(config.colorPrimary.toColorOr(Color.parseColor("#1D4ED8")))
            .setContentTitle(title)
            .setContentText(body)
            // Long messages are the common case for a service notice; without this they are
            // truncated to one line with no way to expand them.
            .setStyle(NotificationCompat.BigTextStyle().bigText(body))
            .setAutoCancel(true)
            .setPriority(NotificationCompat.PRIORITY_HIGH)
            .setDefaults(NotificationCompat.DEFAULT_ALL)
            .setContentIntent(pending)

        // An offer is mostly its artwork, so a news item published with an image shows the
        // picture rather than describing it. The `notification.image` FCM sends is honoured by
        // the system only while the app is in the BACKGROUND; this service builds every tray
        // entry itself, so without fetching it here the image never appeared at all.
        //
        // The fetch is deliberately synchronous: onMessageReceived already runs off the main
        // thread, and posting the notification only after the bitmap is in hand avoids a
        // text-only notification being replaced a moment later. A failure (no network, a
        // 404, an image too large) falls back to the text notification — never to nothing.
        loadBitmap(imageUrl)?.let { bitmap ->
            builder.setLargeIcon(bitmap).setStyle(
                NotificationCompat.BigPictureStyle()
                    .bigPicture(bitmap)
                    // Keeps the tray tidy once expanded: the picture replaces the thumbnail
                    // instead of showing the same image twice.
                    .bigLargeIcon(null as Bitmap?)
                    .setSummaryText(body)
            )
        }

        val notification = builder.build()

        val manager = ContextCompat.getSystemService(this, NotificationManager::class.java) ?: return
        manager.notify(if (notificationId > 0) notificationId else requestCode, notification)
    }

    /**
     * Downloads an alert's picture, or returns null and lets the caller post text only.
     *
     * Only http(s) is followed — the URL arrives in a push payload, and a `file://` or
     * `content://` one would be asking this process to read local storage on a stranger's
     * instruction. The size cap is what Android itself will accept in a notification: a
     * bitmap beyond a few MB is dropped by the system with a "bad array lengths" failure that
     * takes the whole notification with it, so an oversized image is better refused here.
     */
    private fun loadBitmap(imageUrl: String?): Bitmap? {
        val raw = imageUrl?.trim().orEmpty()
        if (raw.isEmpty()) return null

        val parsed = runCatching { URL(raw) }.getOrNull() ?: return null
        if (parsed.protocol !in setOf("http", "https")) return null

        return try {
            val connection = (parsed.openConnection() as HttpURLConnection).apply {
                connectTimeout = IMAGE_TIMEOUT_MS
                readTimeout = IMAGE_TIMEOUT_MS
                instanceFollowRedirects = true
                doInput = true
            }
            try {
                if (connection.responseCode !in 200..299) return null
                val declaredLength = connection.contentLength
                if (declaredLength > MAX_IMAGE_BYTES) return null
                connection.inputStream.use { stream ->
                    val bytes = stream.readAtMost(MAX_IMAGE_BYTES)
                        ?: return null
                    BitmapFactory.decodeByteArray(bytes, 0, bytes.size)
                }
            } finally {
                connection.disconnect()
            }
        } catch (t: Throwable) {
            if (BuildConfig.DEV_TOOLS) Log.w(TAG, "Alert image could not be loaded", t)
            null
        }
    }

    /** Reads at most [limit] bytes; null when the stream turns out to be larger than that. */
    private fun InputStream.readAtMost(limit: Int): ByteArray? {
        val buffer = ByteArrayOutputStream()
        val chunk = ByteArray(16 * 1024)
        while (true) {
            val read = read(chunk)
            if (read < 0) break
            if (buffer.size() + read > limit) return null
            buffer.write(chunk, 0, read)
        }
        return buffer.toByteArray()
    }

    companion object {
        private const val TAG = "PortalMessaging"
        private const val IMAGE_TIMEOUT_MS = 8_000
        private const val MAX_IMAGE_BYTES = 3 * 1024 * 1024

        /** Intent extra carrying the in-app path the alert should open. */
        const val EXTRA_PUSH_URL = "fino.push.url"
        const val EXTRA_PUSH_ID = "fino.push.id"

        /** Clear the tray after the user has opened the app from a notification. */
        fun clearAll(context: Context) {
            ContextCompat.getSystemService(context, NotificationManager::class.java)?.cancelAll()
        }
    }
}
