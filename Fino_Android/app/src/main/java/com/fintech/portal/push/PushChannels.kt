package com.fintech.portal.push

import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.Context
import android.net.Uri
import android.os.Build
import androidx.core.content.ContextCompat

/**
 * Notification channels.
 *
 * Since Android 8 a notification whose `channel_id` does not exist on the device is thrown
 * away silently — no tray entry, no log line, nothing for anyone to debug. Creating these on
 * every launch is cheap and idempotent, and the ids match
 * `StaticModels/NotificationKinds.cs -> NotificationKind.ChannelFor` on the server. If the two
 * ever disagree, notifications vanish, so they are named here in one place.
 *
 * Three channels, not one, because a retailer who mutes marketing must not also mute
 * "your wallet was debited".
 */
object PushChannels {

    const val ALERTS = "fino_alerts"
    const val TRANSACTIONS = "fino_txn"
    const val PROMOTIONS = "fino_promo"

    /** Channel used when a message arrives with no (or an unknown) channel id. */
    const val DEFAULT = ALERTS

    fun ensure(context: Context, brandName: String) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) return

        val manager = ContextCompat.getSystemService(context, NotificationManager::class.java) ?: return
        val brand = brandName.ifBlank { "App" }

        manager.createNotificationChannel(
            NotificationChannel(ALERTS, "$brand alerts", NotificationManager.IMPORTANCE_HIGH).apply {
                description = "Service notices, approvals and support replies"
                enableVibration(true)
            }
        )

        manager.createNotificationChannel(
            NotificationChannel(TRANSACTIONS, "Transactions & wallet", NotificationManager.IMPORTANCE_HIGH).apply {
                description = "Money in, money out, and fund requests"
                enableVibration(true)
            }
        )

        // Deliberately lower importance: promotions should not buzz a phone at the counter.
        manager.createNotificationChannel(
            NotificationChannel(PROMOTIONS, "Offers", NotificationManager.IMPORTANCE_DEFAULT).apply {
                description = "Offers and announcements"
                enableVibration(false)
            }
        )
    }

    /** Only channels this app actually created are safe to post to. */
    fun sanitise(channelId: String?): String = when (channelId) {
        TRANSACTIONS, PROMOTIONS, ALERTS -> channelId
        else -> DEFAULT
    }

    /** Unused today; kept so a client-specific sound can be added without touching callers. */
    fun soundFor(@Suppress("UNUSED_PARAMETER") channelId: String): Uri? = null
}
