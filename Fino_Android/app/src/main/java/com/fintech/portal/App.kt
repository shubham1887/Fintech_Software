package com.fintech.portal

import android.app.Application
import android.os.Build
import android.webkit.WebView
import com.fintech.portal.config.ConfigStore

class App : Application() {

    override fun onCreate() {
        super.onCreate()
        ConfigStore.init(this)

        // Let Chrome DevTools inspect the WebView in debug builds
        if (BuildConfig.DEV_TOOLS && Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            WebView.setWebContentsDebuggingEnabled(true)
        }
    }
}
