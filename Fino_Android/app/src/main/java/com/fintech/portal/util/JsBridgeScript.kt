package com.fintech.portal.util

/**
 * A small JS shim injected into the WebView after every page load.
 *
 * What it gives the site:
 *   - <html class="native-app android-app">  -> show/hide app-only things from CSS
 *     (hiding a "Download our app" banner, padding for the bottom nav, and so on)
 *   - --safe-area-top / --safe-area-bottom CSS variables (notch + gesture bar)
 *   - window.NativeApp -> a clean promise-based API over the raw AndroidBridge
 *
 * The site does NOT have to change anything — this only matters when you want to use
 * a native feature: biometric, share, print, haptics.
 */
object JsBridgeScript {

    @Volatile
    var safeAreaTopPx: Int = 0

    @Volatile
    var safeAreaBottomPx: Int = 0

    fun build(): String = """
(function () {
  if (!window.AndroidBridge) return;
  var B = window.AndroidBridge;
  var root = document.documentElement;

  root.classList.add('native-app', 'android-app');
  root.setAttribute('data-native-platform', 'android');
  root.style.setProperty('--safe-area-top', '${safeAreaTopPx}px');
  root.style.setProperty('--safe-area-bottom', '${safeAreaBottomPx}px');

  if (window.NativeApp && window.NativeApp.__ready) return;

  function parse(raw) { try { return JSON.parse(raw); } catch (e) { return {}; } }

  function once(event, trigger) {
    return new Promise(function (resolve) {
      var handler = function (e) {
        window.removeEventListener('native:' + event, handler);
        resolve(e.detail);
      };
      window.addEventListener('native:' + event, handler);
      try {
        trigger();
      } catch (err) {
        window.removeEventListener('native:' + event, handler);
        resolve({ ok: false, reason: String(err) });
      }
    });
  }

  // ---- Tell pull-to-refresh, correctly, whether the page is at its top ----
  // In an SPA the real scroll usually belongs to an inner div. Without this the native
  // side cannot tell, and treats every swipe as a refresh.
  function reportTop(v) { try { B.setScrollAtTop(!!v); } catch (e) {} }

  function documentAtTop() {
    var d = document.scrollingElement || document.documentElement || document.body;
    return !d || d.scrollTop <= 0;
  }

  function scrollableAncestorAtTop(node) {
    var n = node;
    while (n && n.nodeType === 1 && n !== document.body) {
      var st = window.getComputedStyle(n);
      var oy = st.overflowY;
      if ((oy === 'auto' || oy === 'scroll') && n.scrollHeight > n.clientHeight + 1) {
        return n.scrollTop <= 0;
      }
      n = n.parentElement;
    }
    return documentAtTop();
  }

  document.addEventListener('touchstart', function (e) {
    reportTop(scrollableAncestorAtTop(e.target));
  }, { passive: true, capture: true });

  document.addEventListener('scroll', function (e) {
    var t = e.target;
    if (!t || t === document || t === document.documentElement || t === document.body) {
      reportTop(documentAtTop());
    } else if (typeof t.scrollTop === 'number') {
      reportTop(t.scrollTop <= 0);
    }
  }, { passive: true, capture: true });

  reportTop(documentAtTop());

  var api = {
    __ready: true,
    platform: 'android',
    info: parse(B.getAppInfo()),
    config: parse(B.getConfig()),

    toast: function (m) { B.toast(String(m)); },
    haptic: function (t) { B.haptic(String(t || 'light')); },
    share: function (title, text) { B.share(String(title || ''), String(text || '')); },
    copy: function (text, label) { B.copyToClipboard(String(label || 'copied'), String(text || '')); },
    open: function (url) { B.openExternal(String(url)); },
    whatsapp: function (number, msg) { B.openWhatsApp(String(number || ''), String(msg || '')); },
    call: function (number) { B.call(String(number || '')); },
    print: function (name) { B.printPage(String(name || '')); },
    saveFile: function (name, mime, base64) { B.saveBase64File(String(name || ''), String(mime || ''), String(base64 || '')); },

    secureScreen: function (on) { B.setSecureScreen(!!on); },
    statusBarColor: function (hex) { B.setStatusBarColor(String(hex)); },

    notifications: function () {
      return once('notificationPermission', function () { B.requestNotificationPermission(); });
    },

    /**
     * Push. `pushAvailable()` says whether this install has Firebase configured at all;
     * `requestPushToken()` asks for the device token, which arrives as the `native:pushToken`
     * event (the fetch is asynchronous, so there is nothing to return).
     *
     * The console listens for that event in lib/push.ts and posts the token to
     * AppNotification/RegisterDevice — that is the link that was missing entirely, and why
     * alerts sent from the admin panel never reached an Android phone.
     */
    pushAvailable: function () { try { return !!B.pushAvailable(); } catch (e) { return false; } },
    requestPushToken: function () { try { B.requestPushToken(); } catch (e) {} },

    biometricAvailable: function () { return !!B.biometricAvailable(); },
    biometric: function (title, subtitle) {
      return once('biometric', function () {
        B.biometricAuth(String(title || ''), String(subtitle || ''));
      });
    },

    secure: {
      set: function (k, v) { return !!B.secureSet(String(k), String(v)); },
      get: function (k) { return B.secureGet(String(k)) || null; },
      remove: function (k) { B.secureRemove(String(k)); }
    },

    /** Content dikh gaya -> branding overlay hata do (optional). */
    ready: function () { try { B.contentReady(); } catch (e) {} },

    reload: function () { B.reloadApp(); },
    logout: function () { B.clearSession(); },
    exit: function () { B.exitApp(); },

    on: function (event, cb) {
      window.addEventListener('native:' + event, cb);
      return function () { window.removeEventListener('native:' + event, cb); };
    }
  };

  window.NativeApp = api;
  window.dispatchEvent(new CustomEvent('native:ready', { detail: api.info }));
})();
    """.trimIndent()
}
