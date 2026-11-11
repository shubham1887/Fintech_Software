package com.fintech.portal.ui

import android.annotation.SuppressLint
import android.content.Intent
import android.content.pm.PackageManager
import android.graphics.Color
import android.os.Build
import android.os.Bundle
import android.view.View
import android.view.WindowManager
import android.webkit.CookieManager
import android.webkit.WebSettings
import android.webkit.WebView
import android.widget.EditText
import androidx.activity.OnBackPressedCallback
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.core.content.ContextCompat
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen
import androidx.core.view.ViewCompat
import androidx.core.view.WindowCompat
import androidx.core.view.WindowInsetsCompat
import androidx.core.view.WindowInsetsControllerCompat
import androidx.lifecycle.lifecycleScope
import androidx.webkit.WebSettingsCompat
import androidx.webkit.WebViewFeature
import com.fintech.portal.BuildConfig
import com.fintech.portal.bridge.NativeBridge
import com.fintech.portal.config.AppConfig
import com.fintech.portal.config.ConfigStore
import com.fintech.portal.databinding.ActivityMainBinding
import com.fintech.portal.push.PortalMessagingService
import com.fintech.portal.push.PushManager
import com.fintech.portal.util.JsBridgeScript
import com.fintech.portal.util.darken
import com.fintech.portal.util.isOnline
import com.fintech.portal.util.onColor
import com.fintech.portal.util.toColorOr
import com.fintech.portal.util.toast
import com.fintech.portal.web.DownloadHelper
import com.fintech.portal.web.FileChooser
import com.fintech.portal.web.PortalChromeClient
import com.fintech.portal.web.PortalWebViewClient
import com.fintech.portal.web.UrlRouter
import kotlinx.coroutines.launch

/**
 * The app's only screen.
 *
 * The launch flow deliberately does NOT wait on the network:
 *  1. branding is applied immediately from the cached config
 *  2. the WebView starts loading the page in that same instant
 *  3. the branding overlay sits on top until the page arrives
 *  4. the remote config refreshes in the background
 */
class MainActivity : AppCompatActivity() {

    private lateinit var binding: ActivityMainBinding
    private lateinit var fileChooser: FileChooser

    private var permissionCallback: ((Boolean) -> Unit)? = null
    private var lastBackPress = 0L
    private var pageFailed = false
    private var overlayHidden = false
    private var notificationPermissionAsked = false

    private val config: AppConfig get() = ConfigStore.current

    private val permissionLauncher = registerForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { granted ->
        permissionCallback?.invoke(granted)
        permissionCallback = null
    }

    // ------------------------------------------------------------ lifecycle

    override fun onCreate(savedInstanceState: Bundle?) {
        installSplashScreen()
        super.onCreate(savedInstanceState)
        WindowCompat.setDecorFitsSystemWindows(window, false)

        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        fileChooser = FileChooser(this) { permission, callback ->
            requestPermission(permission, callback)
        }

        applyBranding()
        applyInsets()
        applySecureFlag()
        setupWebView()
        setupRefresh()
        setupBackHandling()

        binding.retryButton.setOnClickListener { retry() }
        binding.supportButton.setOnClickListener { contactSupport() }

        if (BuildConfig.DEV_TOOLS) {
            binding.overlayLogo.setOnLongClickListener {
                showUrlOverrideDialog()
                true
            }
        }

        // Gate check against the cached config — instant, no network
        val blocked = Gates.check(this, config) { retry() }

        if (savedInstanceState != null) {
            binding.webView.restoreState(savedInstanceState)
            hideBrandOverlay()
        } else if (!blocked) {
            loadStartUrl(intent)
        }

        // Make sure the overlay can never get stuck
        binding.root.postDelayed({ hideBrandOverlay() }, OVERLAY_TIMEOUT_MS)

        // Remote config in the background — never in the way of the page load
        refreshConfig()
    }

    override fun onNewIntent(intent: Intent) {
        super.onNewIntent(intent)
        setIntent(intent)

        // A notification tap on a running app must NOT reload the start URL — that would
        // throw away whatever the user was in the middle of. Hand the alert to the page and
        // let the SPA route to it.
        if (intent.hasExtra(PortalMessagingService.EXTRA_PUSH_URL) ||
            intent.hasExtra(PortalMessagingService.EXTRA_PUSH_ID)
        ) {
            deliverPendingPush()
            return
        }

        loadStartUrl(intent)
    }

    override fun onResume() {
        super.onResume()
        binding.webView.onResume()
        if (System.currentTimeMillis() - ConfigStore.lastFetchedAt > CONFIG_TTL_MS) refreshConfig()
    }

    override fun onPause() {
        binding.webView.onPause()
        CookieManager.getInstance().flush()
        super.onPause()
    }

    override fun onSaveInstanceState(outState: Bundle) {
        super.onSaveInstanceState(outState)
        binding.webView.saveState(outState)
    }

    override fun onDestroy() {
        fileChooser.release()
        binding.webView.apply {
            stopLoading()
            removeJavascriptInterface(BRIDGE_NAME)
            (parent as? android.view.ViewGroup)?.removeView(this)
            destroy()
        }
        super.onDestroy()
    }

    // -------------------------------------------------------------- branding

    private fun applyBranding() {
        val primary = config.colorPrimary.toColorOr(Color.parseColor("#1D4ED8"))
        val accent = config.colorAccent.toColorOr(primary)
        val splash = config.colorSplash.toColorOr(primary.darken(0.5f))

        binding.root.setBackgroundColor(primary)
        binding.progress.progressTintList = android.content.res.ColorStateList.valueOf(accent)
        binding.retryButton.backgroundTintList = android.content.res.ColorStateList.valueOf(primary)

        binding.brandOverlay.setBackgroundColor(splash)
        binding.overlayBrand.text = config.brandName
        binding.overlayBrand.setTextColor(splash.onColor())
        binding.overlayProgress.indeterminateTintList =
            android.content.res.ColorStateList.valueOf(accent)

        @Suppress("DEPRECATION")
        window.statusBarColor = primary
        @Suppress("DEPRECATION")
        window.navigationBarColor = Color.WHITE
        WindowInsetsControllerCompat(window, binding.root).apply {
            isAppearanceLightStatusBars = primary.onColor() != Color.WHITE
            isAppearanceLightNavigationBars = true
        }
        binding.swipeRefresh.setColorSchemeColors(primary, accent)
    }

    /**
     * On targetSdk 35 the app runs edge-to-edge, so we handle the notch and gesture-bar
     * space ourselves — and hand the same values to the site as CSS variables.
     */
    private fun applyInsets() {
        ViewCompat.setOnApplyWindowInsetsListener(binding.root) { view, insets ->
            val bars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
            val ime = insets.getInsets(WindowInsetsCompat.Type.ime())
            view.setPadding(bars.left, bars.top, bars.right, maxOf(bars.bottom, ime.bottom))

            val density = resources.displayMetrics.density
            JsBridgeScript.safeAreaTopPx = (bars.top / density).toInt()
            JsBridgeScript.safeAreaBottomPx = (bars.bottom / density).toInt()
            insets
        }
    }

    private fun applySecureFlag() {
        if (config.blockScreenshots) {
            window.setFlags(
                WindowManager.LayoutParams.FLAG_SECURE,
                WindowManager.LayoutParams.FLAG_SECURE
            )
        }
    }

    // --------------------------------------------------------------- webview

    @SuppressLint("SetJavaScriptEnabled")
    private fun setupWebView() {
        val webView = binding.webView

        webView.settings.apply {
            javaScriptEnabled = true
            domStorageEnabled = true
            databaseEnabled = true
            javaScriptCanOpenWindowsAutomatically = true
            setSupportMultipleWindows(true)
            loadWithOverviewMode = true
            useWideViewPort = true
            setSupportZoom(false)
            builtInZoomControls = false
            displayZoomControls = false
            mediaPlaybackRequiresUserGesture = false
            cacheMode = WebSettings.LOAD_DEFAULT

            // Security: never run http content on an https page
            mixedContentMode = WebSettings.MIXED_CONTENT_NEVER_ALLOW
            allowFileAccess = false
            allowContentAccess = false

            userAgentString = userAgentString + " " + config.userAgentSuffix
        }

        // The site handles its own theme — turn off the WebView's forced dark
        if (WebViewFeature.isFeatureSupported(WebViewFeature.ALGORITHMIC_DARKENING)) {
            WebSettingsCompat.setAlgorithmicDarkeningAllowed(webView.settings, false)
        }

        CookieManager.getInstance().apply {
            setAcceptCookie(true)
            setAcceptThirdPartyCookies(webView, true)
        }

        // The overscroll glow interferes with the WebView's own scrolling
        webView.overScrollMode = View.OVER_SCROLL_NEVER
        webView.isScrollbarFadingEnabled = true
        webView.setBackgroundColor(Color.WHITE)

        webView.webViewClient = PortalWebViewClient(
            activity = this,
            onStart = {
                pageFailed = false
                setProgressVisible(true)
            },
            onFinish = {
                setProgressVisible(false)
                binding.swipeRefresh.isRefreshing = false
                if (!pageFailed) {
                    showOffline(false)
                    hideBrandOverlay()
                }
                // The bridge is injected with the page, so this is the first moment the
                // console can be told anything. Hand it the push token, then the notification
                // that opened the app (if any) — in that order, so the console has registered
                // the device before it acts on the tap.
                deliverPushToken()
                deliverPendingPush()
            },
            onError = { message ->
                pageFailed = true
                binding.swipeRefresh.isRefreshing = false
                setProgressVisible(false)
                hideBrandOverlay()
                showOffline(true, message)
            }
        )

        webView.webChromeClient = PortalChromeClient(
            activity = this,
            fileChooser = fileChooser,
            onProgress = { progress ->
                binding.progress.progress = progress
                setProgressVisible(progress in 1..99)
                if (progress >= 95 && !pageFailed) hideBrandOverlay()
            },
            onTitle = { },
            requestPermission = { permission, callback -> requestPermission(permission, callback) }
        )

        if (config.allowDownloads) {
            webView.setDownloadListener { url, userAgent, disposition, mime, _ ->
                DownloadHelper.handle(this, webView, url, userAgent, disposition, mime)
            }
        }

        webView.addJavascriptInterface(
            NativeBridge(
                activity = this,
                webView = webView,
                requestPermission = { permission, callback -> requestPermission(permission, callback) },
                onLogout = { clearSessionAndRestart() },
                onScrollAtTop = { atTop -> binding.swipeRefresh.pageAtTop = atTop },
                onContentReady = { hideBrandOverlay() }
            ),
            BRIDGE_NAME
        )
    }

    /**
     * Pull-to-refresh defaults to OFF (clients.json). In an SPA it fights the page's own
     * scrolling, so unless it is genuinely wanted it is better off — a disabled
     * SwipeRefreshLayout intercepts no touches at all and scrolling stays native.
     */
    private fun setupRefresh() {
        binding.swipeRefresh.webView = binding.webView
        binding.swipeRefresh.isEnabled = config.pullToRefresh
        binding.swipeRefresh.setOnRefreshListener {
            if (isOnline()) {
                binding.webView.reload()
            } else {
                binding.swipeRefresh.isRefreshing = false
                toast("No internet connection")
            }
        }
    }

    private fun setupBackHandling() {
        onBackPressedDispatcher.addCallback(this, object : OnBackPressedCallback(true) {
            override fun handleOnBackPressed() {
                when {
                    binding.offlineView.visibility == View.VISIBLE && binding.webView.canGoBack() -> {
                        showOffline(false)
                        binding.webView.goBack()
                    }

                    binding.webView.canGoBack() -> binding.webView.goBack()

                    !config.confirmExit -> finish()

                    System.currentTimeMillis() - lastBackPress < EXIT_WINDOW_MS -> finish()

                    else -> {
                        lastBackPress = System.currentTimeMillis()
                        toast("Press back again to exit")
                    }
                }
            }
        })
    }

    // ----------------------------------------------------------- navigation

    private fun loadStartUrl(intent: Intent?) {
        val deepLink = intent?.data?.toString()
        val target = if (!deepLink.isNullOrBlank() && config.isInternalHost(intent.data?.host)) {
            deepLink
        } else {
            config.startUrl()
        }

        if (target.isBlank()) {
            hideBrandOverlay()
            showOffline(true, "Portal URL is not configured")
            return
        }

        if (!isOnline()) {
            hideBrandOverlay()
            showOffline(true, "No internet connection")
            return
        }

        binding.webView.loadUrl(target)
    }

    // ------------------------------------------------------------------ push

    /**
     * Ask for tray permission once, on Android 13+.
     *
     * Deliberately NOT on the very first frame: a permission dialog before the user has seen
     * what the app is gets refused, and Android only offers it twice. This runs after the
     * console has loaded and the user is signed in, which is also when the alerts start being
     * worth receiving.
     */
    private fun requestNotificationPermissionIfNeeded() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.TIRAMISU) return
        if (notificationPermissionAsked) return

        val granted = ContextCompat.checkSelfPermission(
            this, android.Manifest.permission.POST_NOTIFICATIONS
        ) == PackageManager.PERMISSION_GRANTED
        if (granted) return

        notificationPermissionAsked = true
        requestPermission(android.Manifest.permission.POST_NOTIFICATIONS) { allowed ->
            if (allowed) deliverPushToken()
        }
    }

    /** Push the FCM token into the page as a `native:pushToken` event. */
    private fun deliverPushToken() {
        // A fresh config may have switched push on since launch.
        PushManager.init(this, config)
        if (!PushManager.isReady) return

        requestNotificationPermissionIfNeeded()

        PushManager.fetchToken { token ->
            val payload = org.json.JSONObject()
                .put("token", token)
                .put("configured", true)
                .put("platform", "android")
                .put("device", Build.MANUFACTURER + " " + Build.MODEL)
                .put("appVersion", BuildConfig.VERSION_NAME)
            emitToPage("pushToken", payload)
        }
    }

    /**
     * If the app was opened by tapping a notification, tell the page which alert it was so it
     * can navigate there and refresh the bell. Consumed once — the extras are stripped so a
     * later rotation or resume does not replay the same navigation.
     */
    private fun deliverPendingPush() {
        val source = intent ?: return
        val url = source.getStringExtra(PortalMessagingService.EXTRA_PUSH_URL).orEmpty()
        val id = source.getIntExtra(PortalMessagingService.EXTRA_PUSH_ID, 0)
        if (url.isBlank() && id <= 0) return

        source.removeExtra(PortalMessagingService.EXTRA_PUSH_URL)
        source.removeExtra(PortalMessagingService.EXTRA_PUSH_ID)

        emitToPage(
            "push",
            org.json.JSONObject()
                .put("notificationId", id)
                .put("url", url)
                .put("foreground", false)
        )
        PortalMessagingService.clearAll(this)
    }

    /** Fire a `native:<event>` CustomEvent inside the WebView. */
    private fun emitToPage(event: String, payload: org.json.JSONObject) {
        val js = "window.dispatchEvent(new CustomEvent('native:$event',{detail:$payload}));"
        runOnUiThread { binding.webView.evaluateJavascript(js, null) }
    }

    private fun retry() {
        if (!isOnline()) {
            toast("Still no internet connection")
            return
        }
        showOffline(false)
        if (binding.webView.url.isNullOrBlank()) loadStartUrl(intent) else binding.webView.reload()
    }

    private fun showOffline(show: Boolean, message: String = "") {
        binding.offlineView.visibility = if (show) View.VISIBLE else View.GONE
        binding.swipeRefresh.visibility = if (show) View.INVISIBLE else View.VISIBLE
        if (show) {
            binding.offlineTitle.text =
                if (isOnline()) "Page could not be loaded" else "No internet connection"
            binding.offlineMessage.text = message.ifBlank {
                "Check your network and try again."
            }
        }
    }

    private fun hideBrandOverlay() {
        if (overlayHidden) return
        overlayHidden = true
        binding.brandOverlay.animate()
            .alpha(0f)
            .setDuration(200L)
            .withEndAction { binding.brandOverlay.visibility = View.GONE }
            .start()
    }

    private fun setProgressVisible(visible: Boolean) {
        val want = if (visible) View.VISIBLE else View.GONE
        if (binding.progress.visibility != want) binding.progress.visibility = want
    }

    private fun contactSupport() {
        val number = config.supportWhatsApp
        if (number.isBlank()) {
            toast("Support number is not configured")
            return
        }
        UrlRouter.openWhatsApp(
            this,
            number,
            "Hello, I need help with the " + config.brandName + " app."
        )
    }

    private fun clearSessionAndRestart() {
        CookieManager.getInstance().removeAllCookies(null)
        CookieManager.getInstance().flush()
        binding.webView.clearHistory()
        binding.webView.clearCache(true)
        // Drop the push token too. A retail counter phone is shared, and the outgoing user's
        // alerts must not keep landing on it. (The console has already told the API to stop
        // pushing to this token, while its session was still valid.)
        PushManager.clearToken()
        PortalMessagingService.clearAll(this)
        loadStartUrl(null)
    }

    // -------------------------------------------------------------- helpers

    private fun requestPermission(permission: String, callback: (Boolean) -> Unit) {
        if (ContextCompat.checkSelfPermission(this, permission) == PackageManager.PERMISSION_GRANTED) {
            callback(true)
            return
        }
        if (permission == android.Manifest.permission.POST_NOTIFICATIONS &&
            Build.VERSION.SDK_INT < Build.VERSION_CODES.TIRAMISU
        ) {
            callback(true)
            return
        }
        permissionCallback = callback
        permissionLauncher.launch(permission)
    }

    /** Fetch the config in the background and apply what changed without a reload. */
    private fun refreshConfig() {
        lifecycleScope.launch {
            if (!ConfigStore.refresh() || isFinishing) return@launch
            applyBranding()
            binding.swipeRefresh.isEnabled = ConfigStore.current.pullToRefresh
            Gates.check(this@MainActivity, ConfigStore.current) { retry() }
        }
    }

    private fun showUrlOverrideDialog() {
        val input = EditText(this).apply {
            hint = "https://staging.example.com"
            setText(ConfigStore.devUrlOverride)
            setPadding(48, 32, 48, 32)
        }
        AlertDialog.Builder(this)
            .setTitle("Dev: portal URL override")
            .setMessage("Leave empty to use the URL from the build.")
            .setView(input)
            .setPositiveButton("Save") { _, _ ->
                ConfigStore.devUrlOverride = input.text.toString()
                recreate()
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    companion object {
        private const val BRIDGE_NAME = "AndroidBridge"
        private const val EXIT_WINDOW_MS = 2000L
        private const val CONFIG_TTL_MS = 15 * 60 * 1000L
        private const val OVERLAY_TIMEOUT_MS = 12_000L
    }
}
