package com.fintech.portal.ui

import android.app.Activity
import android.content.Intent
import android.net.Uri
import androidx.appcompat.app.AlertDialog
import com.fintech.portal.BuildConfig
import com.fintech.portal.config.AppConfig

/**
 * Two gates driven by the remote config — both can be switched on or off from the server
 * without releasing a new APK:
 *
 *  1) Maintenance mode  -> the app will not open at all, only the message shows
 *  2) Force update      -> purana versionCode block, Play Store bhejo
 *
 * app-config.json:
 *   { "maintenance": true, "maintenanceMessage": "..." }
 *   { "minVersionCode": 5, "forceUpdate": true, "updateUrl": "https://play..." }
 */
object Gates {

    /** true = the app cannot continue; the dialog has been shown. */
    fun check(activity: Activity, config: AppConfig, onRetry: () -> Unit): Boolean {
        if (config.maintenance) {
            showMaintenance(activity, config, onRetry)
            return true
        }

        val outdated = BuildConfig.VERSION_CODE < config.minVersionCode
        if (outdated) {
            showUpdate(activity, config)
            return config.forceUpdate
        }
        return false
    }

    private fun showMaintenance(activity: Activity, config: AppConfig, onRetry: () -> Unit) {
        if (activity.isFinishing) return
        AlertDialog.Builder(activity)
            .setTitle("Under maintenance")
            .setMessage(
                config.maintenanceMessage.ifBlank {
                    "We are upgrading the system. Please try again shortly."
                }
            )
            .setCancelable(false)
            .setPositiveButton("Try again") { d, _ ->
                d.dismiss()
                onRetry()
            }
            .setNegativeButton("Close") { _, _ -> activity.finishAffinity() }
            .show()
    }

    private fun showUpdate(activity: Activity, config: AppConfig) {
        if (activity.isFinishing) return
        val builder = AlertDialog.Builder(activity)
            .setTitle("Update available")
            .setMessage(
                config.updateMessage.ifBlank {
                    "Update the app for the latest fixes and security improvements."
                }
            )
            .setCancelable(!config.forceUpdate)
            .setPositiveButton("Update") { _, _ ->
                runCatching {
                    activity.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(config.updateUrl)))
                }
                if (config.forceUpdate) activity.finishAffinity()
            }

        if (!config.forceUpdate) {
            builder.setNegativeButton("Later") { d, _ -> d.dismiss() }
        } else {
            builder.setNegativeButton("Close") { _, _ -> activity.finishAffinity() }
        }
        builder.show()
    }
}
