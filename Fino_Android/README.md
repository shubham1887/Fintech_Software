# Fintech Portal — Android App (WebView, white-label)

One codebase, **any number of client apps**. Each client gets its own name, logo, colours,
package id and portal URL — and a great deal can still be changed from the server after
launch, without releasing a new APK.

What the app itself does: runs your React portal (`Fino_Web`) inside a proper native shell —
file upload, downloads, print, biometric, deep links, an offline screen, force-update,
maintenance mode and screenshot blocking are all handled on the native side.

---

## 1. Two layers of configuration

| | **Build-time** | **Runtime** |
|---|---|---|
| File | `whitelabel/clients.json` | `https://<portal>/app-config.json` |
| Applied | when the APK is built | on every app launch |
| Controls | app name, package id, launcher icon, version | URL, **start page**, colours, brand name, feature toggles, force-update, maintenance, support numbers |
| New APK needed? | Yes | **No** |

In practice: hand the client their APK once. After that their name, colours, support number
and even "maintenance mode" are all driven by a single JSON file on their own server.

---

## 2. Adding a client

```powershell
.\tools\new-client.ps1 -Id sharma -Name "Sharma Pay" -ApplicationId com.sharmapay.app -Url https://sharmapay.in -Primary "#7C3AED" -Accent "#A78BFA" -WhatsApp 919812345678
```

The script:

1. adds the entry to `whitelabel/clients.json`
2. creates `app/src/sharma/res/drawable/` (for the logo override)
3. prints the build command

Then:

```bash
gradlew assembleSharmaRelease
```

To change the logo, drop these two files into that folder (keeping the same names):

- `ic_brand_mark.xml` — splash / loading-overlay logo
- `ic_launcher_foreground.xml` — launcher icon foreground

(Android Studio's **Image Asset Studio** is the easiest way to produce both, including PNG
and adaptive-icon variants if you want them.)

The script is a convenience, not a requirement — add a block to the `clients` array in
`whitelabel/clients.json` and Gradle generates that client's product flavor by itself.

---

## 3. Runtime config (`app-config.json`)

Host it at the portal root on the client's web server: `https://sharmapay.in/app-config.json`
Sample: `whitelabel/app-config.sample.json`

```json
{
  "brandName": "Sharma Pay",
  "portalUrl": "https://sharmapay.in",
  "startPath": "/login",
  "internalHosts": ["sharmapay.in", "app.sharmapay.in"],

  "colorPrimary": "#7C3AED",
  "colorAccent": "#A78BFA",
  "colorSplash": "#1E1B4B",

  "pullToRefresh": false,
  "blockScreenshots": true,
  "allowDownloads": true,
  "confirmExit": true,

  "minVersionCode": 3,
  "forceUpdate": true,
  "updateUrl": "https://play.google.com/store/apps/details?id=com.sharmapay.app",

  "maintenance": false,
  "maintenanceMessage": "We are upgrading the system. Please try again in 30 minutes.",

  "supportWhatsApp": "919812345678"
}
```

Every key is optional — anything you leave out keeps the default baked into the APK.
If the config cannot be fetched, or there is no internet, the app runs on the **last
successful config** from its cache. Launch never waits on it: the page starts loading
immediately and the config applies itself in the background when it arrives.

The switches worth knowing:

- **`startPath`** — which page opens on launch. Default `/login`. This is the most important
  setting here: in Fino_Web `/` is the public marketing site (heavy), while the app proper
  starts at `/login`. Get it wrong and the app feels slow.
- **`pullToRefresh`** — defaults to **false**. Inside an SPA it fights the page's own
  scrolling (see "Scroll and speed" below). Turn it on only if you actually want it.
- **`maintenance: true`** — the app will not open at all, just a message and a retry. Customers
  see that instead of a blank screen during downtime.
- **`minVersionCode` + `forceUpdate`** — send users on older builds to the Play Store.
- **`blockScreenshots`** — blocks screenshots and screen recording (FLAG_SECURE) on screens
  showing money.
- **`internalHosts`** — only these domains open inside the app. Every other link goes to a
  Chrome Custom Tab, so a phishing page can never render inside the app chrome.

---

## 4. Native features (optional for the website)

The app injects a small JS shim on every page load. The website does **not** have to change
anything — this only matters when you want a native feature.

What you get for free:

```css
/* when running inside the app */
html.native-app .download-app-banner { display: none; }
.header { padding-top: var(--safe-area-top); }            /* notch */
.bottom-bar { padding-bottom: var(--safe-area-bottom); }  /* gesture bar */
```

And `window.NativeApp`:

```js
NativeApp.info            // { platform, versionName, versionCode, device, ... }
NativeApp.config          // { portalUrl, brandName, colorPrimary, supportWhatsApp, ... }

NativeApp.toast("Recharge successful")
NativeApp.haptic("success")                       // light | medium | heavy | success
NativeApp.share("Receipt", "Txn ID: 12345 ...")
NativeApp.copy("9876543210", "Mobile")
NativeApp.print("Recharge receipt")               // print, or save as PDF
NativeApp.saveFile("report.csv", "text/csv", b64) // into the Downloads folder
NativeApp.whatsapp("", "I need help")             // empty number = the support number
NativeApp.call("")                                // empty = the support number
NativeApp.open("https://external.site")           // in a Custom Tab

NativeApp.secureScreen(true)                      // block screenshots on this screen
NativeApp.statusBarColor("#7C3AED")

await NativeApp.notifications()                   // { granted: true|false }

// Push notifications
NativeApp.pushAvailable()                         // is Firebase configured for this client?
NativeApp.requestPushToken()                      // answers with the 'pushToken' event

NativeApp.on("pushToken", e => e.detail.token)    // register it: AppNotification/RegisterDevice
NativeApp.on("push",      e => e.detail.url)      // a push arrived, or the user tapped one

// Biometric login
if (NativeApp.biometricAvailable()) {
  const r = await NativeApp.biometric("Login", "Unlock with your fingerprint")
  if (r.ok) {
    const token = NativeApp.secure.get("session")  // Keystore-encrypted
    // ...auto-login
  }
}
NativeApp.secure.set("session", token)
NativeApp.secure.remove("session")

NativeApp.ready()     // "my content is ready" -> drop the branding overlay now
NativeApp.reload()
NativeApp.logout()    // clear cookies + cache and go back to login
NativeApp.exit()

const off = NativeApp.on("ready", e => console.log(e.detail))
```

`NativeApp.secure` encrypts with an AES-256-GCM key held in the Android Keystore and stores
the result in app-private storage — the key cannot be extracted from the device.

---

## 5. Build

Requires **JDK 17+** and **Android SDK 35**. Open the `Fino_Android` folder in Android Studio
(Ladybug or newer) and it will handle the rest.

```bash
gradlew assembleFinoDebug        # Fino client, debug APK
gradlew assembleFinoRelease      # Fino client, release APK (needs signing)
gradlew bundleFinoRelease        # .aab for the Play Store
gradlew tasks --all              # tasks for every client
```

The APK lands in `app/build/outputs/apk/<client>/<buildType>/`

Debug builds get a `.debug` suffix on the package (e.g. `com.finoconsole.app.debug`), so a
debug and a release build can sit on the same phone.

### Release signing

Create `keystore.properties` in the project root (**never commit it**):

```properties
storeFile=keys/sharmapay.jks
storePassword=xxxx
keyAlias=sharmapay
keyPassword=xxxx
```

To create the keystore:

```bash
keytool -genkeypair -v -keystore keys/sharmapay.jks -keyalg RSA -keysize 2048 -validity 10000 -alias sharmapay
```

Give every client **its own keystore**, and keep it safe — lose it and you can never publish
an update for that client's app again.

---

## 6. Local development

Point the `demo` client's URL in `whitelabel/clients.json` at your dev server, or — easier —
in a debug APK **long-press the logo on the branding overlay**; a dialog opens where you can
type any URL:

- laptop's Vite dev server from the emulator: `http://10.0.2.2:5173`
- a real phone on the same wifi: `http://192.168.x.x:5173`

Plain HTTP is allowed only for `localhost` / `10.0.2.2` / `127.0.0.1`
(`res/xml/network_security_config.xml`) — production is HTTPS-only.

To debug the WebView from Chrome: open `chrome://inspect` with the phone connected over USB.
Inspection is enabled in debug builds.

---

## 7. Where the code lives

```
app/src/main/java/com/fintech/portal/
├── App.kt                        Application, config init
├── config/
│   ├── AppConfig.kt              every setting + build-time defaults + merge logic
│   └── ConfigStore.kt            remote config fetch, cache, dev URL override
├── ui/
│   ├── MainActivity.kt           the only screen: WebView host, branding overlay,
│   │                             insets, back, refresh, offline
│   └── Gates.kt                  maintenance + force-update dialogs
├── web/
│   ├── PortalWebViewClient.kt    navigation, errors, SSL (invalid cert = reject)
│   ├── PortalChromeClient.kt     progress, file chooser, camera/location, popups
│   ├── UrlRouter.kt              internal vs Custom Tab vs another app (upi:, tel:, intent:)
│   ├── PortalSwipeRefreshLayout.kt  SPA-aware pull-to-refresh (the scroll fix)
│   ├── FileChooser.kt            <input type=file> — camera + gallery, multi-select
│   └── DownloadHelper.kt         http / blob: / data: downloads
├── bridge/NativeBridge.kt        window.AndroidBridge (callable from JS)
└── util/
    ├── JsBridgeScript.kt         window.NativeApp shim + CSS safe-area vars
    ├── SecureStore.kt            Keystore AES-256-GCM key-value store
    ├── Net.kt                    config fetch
    └── Ext.kt                    colour + network helpers
```

---

## 8. Scroll and speed

Two things are set the way they are on purpose:

**Launch never waits on the network.** There used to be a separate SplashActivity that blocked
until `app-config.json` arrived (up to 3.5 seconds) — that is what made the app feel slow. Now
MainActivity is the launcher: branding is applied instantly from the cached config, the WebView
starts loading in that same moment, the branding overlay sits on top, and the remote config
updates in the background. The website can call `NativeApp.ready()` to drop the overlay itself.

**Pull-to-refresh defaults to OFF.** Wrapping a WebView in a SwipeRefreshLayout makes scrolling
stick: it reads `webView.scrollY` to decide whether the page is at the top, but in a React SPA
the real scroll belongs to an inner `overflow-y: auto` div and the WebView's own scrollY stays
0 — so every swipe becomes a pull-to-refresh. A disabled SwipeRefreshLayout intercepts no
touches at all and scrolling stays perfectly native.

To turn it on, set `"pullToRefresh": true`. `PortalSwipeRefreshLayout` now asks the page
directly (a JS `touchstart`/`scroll` listener tells the native side whether the element being
scrolled is at its top), so it no longer has the old problem.

If it still feels slow, the cause is usually outside the app:

- wrong `startPath` (the marketing page is loading — it should be `/login`)
- no cache headers on the portal's static assets, so the whole JS bundle downloads every launch
- `app-config.json` is not on the server at all and the request hangs until it times out

All three are visible in the Network tab of `chrome://inspect` against a debug build.

## 9. Pre-Play-Store checklist

- [ ] A separate keystore per client, backed up somewhere safe
- [ ] `versionCode` bumped for every release (`whitelabel/clients.json`)
- [ ] `app-config.json` is live on the client's server, over HTTPS
- [ ] Privacy policy URL (required by Play Console — and expected for fintech)
- [ ] Data safety form: the app accesses camera, location and files
- [ ] Launcher icon and splash logo are the client's own, not the default mark
- [ ] Install the release APK on a phone and run the whole flow: login → recharge →
      report download → KYC image upload → logout

## 10. Push notifications (FCM)

Push is built in, and it is configured at **run time** — there is no `google-services.json`
in this repo and the `com.google.gms.google-services` Gradle plugin is deliberately not
applied. One APK serves every client; switching push on for a new client is an edit to that
client's hosted `app-config.json`, not a rebuild and a Play Store release.

**Per client, once:**

1. Firebase Console → add a project (or reuse the client's) → add an **Android app** with
   this client's `applicationId` from `whitelabel/clients.json`.
2. Open the generated `google-services.json` and copy four values into the client's
   `app-config.json`:

   | app-config.json | google-services.json |
   |---|---|
   | `fcmProjectId`  | `project_info.project_id` |
   | `fcmAppId`      | `client[0].client_info.mobilesdk_app_id` |
   | `fcmApiKey`     | `client[0].api_key[0].current_key` |
   | `fcmSenderId`   | `project_info.project_number` |

3. Server side: Firebase Console → Project settings → **Service accounts** → *Generate new
   private key*. Put that JSON on the API server and point `FCM:ServiceAccountFile` at it in
   `appsettings.json` (or paste it into `FCM:ServiceAccountJson`).

Leave any of it blank and nothing breaks: alerts are still stored and shown in the app's
notification bell, they just don't reach the phone's tray.

**How it flows**

```
Admin → App Alert  ─▶  AppNotification/Send      (row per recipient — the durable part)
                            │
                            └▶ FCM HTTP v1 ──▶ PortalMessagingService ──▶ tray
                                                        │ tap
                                                        ▼
                                          MainActivity → native:push → console navigates
```

The token flows the other way: `PushManager` → `native:pushToken` → the console's
`lib/push.ts` → `AppNotification/RegisterDevice` → a `UserDevices` row.

**Channels** (`push/PushChannels.kt`) are `fino_alerts`, `fino_txn` and `fino_promo`, and the
ids must stay identical to `NotificationKind.ChannelFor` on the server — Android silently
discards a notification whose channel does not exist, with no error anywhere.

---

## 11. Not included yet (say the word)

- **QR / barcode scanner** — scanning UPI QRs needs ML Kit or ZXing, which grows the app size,
  so it is not in yet.
- **SMS OTP autofill** — needs Google's SMS Retriever, and the backend's SMS template has to
  carry the app hash.
