package com.fintech.portal

import android.app.Application
import android.os.Build
import android.webkit.WebView
import com.fintech.portal.config.ConfigStore
import com.fintech.portal.push.PushChannels
import com.fintech.portal.push.PushManager

class App : Application() {

    override fun onCreate() {
        super.onCreate()
        ConfigStore.init(this)

        // Channels first, and unconditionally: a push can arrive before the app is ever
        // opened, and since Android 8 a notification whose channel does not exist is dropped
        // without a trace. Creating them is idempotent.
        PushChannels.ensure(this, ConfigStore.current.brandName)

        // Firebase from the cached config. If this client has push configured, the token is
        // available from the first launch; if not, this is a no-op and nothing else changes.
        PushManager.init(this, ConfigStore.current)

        // Let Chrome DevTools inspect the WebView in debug builds
        if (BuildConfig.DEV_TOOLS && Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            WebView.setWebContentsDebuggingEnabled(true)
        }
    }
}
