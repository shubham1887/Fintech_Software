# Deployment kit

Installing Fino for a new client, repeatedly, without a developer on the call.

Two people are involved:

* **You** run one command on your own machine to produce a release zip.
* **Whoever is at the client's server** unzips it and double-clicks one file. They do not need to
  know what IIS, Node or .NET are.

---

## 1. What this replaces

The manual sequence — install IIS, install Node, install the .NET hosting bundle, create the API
and UI sites, create their folders, copy the published files in, edit `appsettings.json`, edit the
UI's config, run the SQL scripts to create the tables, start the sites — is gone. All of it.

Why each step disappeared:

| Old step | Why it is no longer needed |
|---|---|
| Install IIS | The application hosts itself as a Windows service (Kestrel). There is no web server to configure. |
| Install the .NET runtime / hosting bundle | The release is published **self-contained** — the runtime is inside the folder. |
| Install Node.js | The operator console is built before shipping. The server only serves files. |
| Two IIS sites, two folders | One service serves the panel at `/` and the API at `/api/`. One address, one certificate, no CORS. |
| Edit `appsettings.json` by hand | The installer writes `appsettings.Production.json` from the answers it collects. |
| Edit the UI's config by hand | Written at the same time, with the matching AES key. |
| Run SQL scripts to create tables | The application builds its own database on first start. |

What is still per-client, and therefore still asked: **company name, logo, admin login, address,
port.**

```
          https://panel.client.com
                    |
              +-----v------+
              |  one       |   Windows service "FinoAPI"
              |  service   |
              |  /         | -> the operator console (React, pre-built)
              |  /api/*    | -> the API (.NET 9, self-contained)
              |  /health   | -> "is it alive", used by the installer
              +-----v------+
                    |
              +-----v------+
              | SQL Server |   created, built and seeded on first start
              +------------+
```

---

## 2. The one file: `FinoSetup.exe`

Double-click it. Windows asks for administrator rights (the manifest requests them, so the prompt
comes first rather than after three screens), and a window opens.

`Setup.bat` is the same thing in a console, for anyone who prefers it. **Both run the same
`Setup.ps1`** — the graphical setup collects the answers into a file and hands them over, so there
is one installation to keep correct and two ways to reach it.

What the window shows:

| Screen | What it does |
|---|---|
| Home | What this server is (Windows, memory, disk, SQL Server) and what is on it already, with the buttons that make sense for that state |
| Client details | Company name, logo (with preview), the owner's login, address, port, database. Validated before anything runs |
| Progress | The live log, coloured, with the current step called out. This is the same output the console shows |
| Done / Failed | "Open the panel", "Open the summary", or "Save the log to a file" to send to support |

It works out for itself what it is looking at:

| What it finds | What it does |
|---|---|
| A clean server | Full install. Offers to download and install SQL Server Express if there is no database engine. |
| An installation from this kit | Shows installed version vs this release, then a menu: **update**, **change logo/name**, **backup**, or quit. |
| An older hand-made IIS deployment | Recognises it, offers to move it onto the new hosting: same database, keys carried over so nobody is logged out. Backs up first, and never touches the IIS sites without a yes. |

### A new server, start to finish

1. Unzip the whole release onto the server.
2. Double-click `FinoSetup.exe`, click Yes at the Windows prompt.
3. Click **Install on this server**, fill in the form (company name, logo, the owner's login,
   address, port), click **Install now**.
4. Wait. SQL Server Express, if it has to be installed, is 10–20 minutes. Building the database is
   another 3–5.
5. Click **Open the panel**. The summary is written to `INSTALLATION-SUMMARY.txt`.

Along the way it also: generates this installation's own JWT secret and AES key/IV, creates the
database and grants **only** the service account rights on it, locks the config file down to SYSTEM
and Administrators, registers the service with restart-on-failure, opens the firewall port, waits
for `/health`, removes the first-login password from disk, and schedules a nightly backup at 02:00.

### Your own repeat installs

```powershell
.\Setup.ps1 -Unattended -BrandName "RiPay" -AdminMobile "9876543210" `
    -AdminPassword "Str0ng#Pass" -PublicUrl "https://panel.ripay.co.in" `
    -LogoPath "D:\clients\ripay\logo.png"
```

---

## 3. Branding: what to ask the client for

Three things. Nothing else is client-specific.

| | |
|---|---|
| **Company name** | As it should appear in the panel |
| **Logo** | **PNG, JPG or SVG.** A `.cdr`, `.ai` or `.psd` cannot be used on a web page — setup says so and prints what to ask their designer for ("File → Export → PNG, about 512×512, transparent background"). You can finish the install without a logo and add it later. |
| **Admin login** | Mobile, email, password for their owner account |

From one logo file the setup produces the panel logo, the public-site logo and the browser-tab
icon (a square favicon, letterboxed so a wide logo is not squashed). The original file is kept in
`app\wwwroot\uploads\branding\` in case you need it again.

**The artwork is trimmed first.** Designers export logos with generous whitespace — the file this
was written for carried 15% of its height as empty space above the mark — and that margin eats the
height a header gives the logo, so the mark comes out a third smaller than the box it sits in.
Setup crops to the ink before resizing (transparent margins and flat-colour backgrounds both), so
the logo fills the space it is given. That, not a taller header, is what makes a logo look bigger.

**A square app icon is optional and separate.** A client who has one (the tile their Android app
is built around) can pass it with `-AppIconPath`; it is published as `appIconUrl` and shown inside
the phone app in the app bar and on loading screens. The login screen and the sidebar always show
the wide logo, in the app as much as in a browser — those are the places the company name has to
be readable.

**The logo also chooses the colours.** Nobody sends a hex code with their artwork, so the setup
reads one off it: white, black and grey pixels are ignored, the remaining colour is bucketed by
hue, and the biggest bucket — the colour a person would point at — becomes the brand colour, with
its lightness normalised so it can hold white text. The sidebar and header become dark shades of
the same hue, and the phone app is handed the same three values. The result is written into
`config.json` as `brandColor` / `navbarColor` / `appbarColor`, which an operator can override in
**Settings → Appearance** at any time.

**How the logo is shown** depends on its shape, measured in the browser:

| The client sent | What the panel shows |
|---|---|
| A **wide** logo (the usual kind, company name drawn into the artwork) | The logo alone, at the full height of the header. The name is not printed beside it — the artwork already says it. |
| A **square** logo (a symbol with no name in it) | The logo, with the company name beside it. |
| No logo | The built-in mark in the brand colour, with the company name. |

On dark surfaces (sidebar, app bar) the logo is set on a small white card. Client logos are drawn
for white paper; a navy logo on a navy sidebar is invisible, which is exactly what happened the
first time a real one was applied.

**Changing it later** — `FinoSetup.exe` → **Change logo / name** (or `Set-Branding.ps1`). Takes
seconds: the panel reads its branding in the browser, so there is no rebuild and no restart. Tell
the client to press Ctrl+F5 once.

---

## 4. Versions and updating

Every release carries a `version.json`; every installation records what it has in
`install-state.json` (version, brand, service name, port, database). Setup compares the two, warns
if you are about to install something **older** than what is there, and refuses nothing silently.

An update: backup → keep a rollback copy → stop → swap the program files → start → wait for
`/health`. If the new build does not come up, **the previous one is put back automatically** and
the platform is running again before you have finished reading the error.

Never overwritten by an update: `appsettings.Production.json`, `wwwroot\uploads` (KYC documents,
icons), and the client's branding (`config.json`, favicon).

New database changes in a release are applied by the application itself on the first start after
the swap.

---

## 5. Making a release (you, once per version)

On a machine with .NET 9 and Node 22:

```powershell
pwsh .\deploy\Build-Release.ps1 -Version 2026.09.16.1
```

Produces **one** `deploy\dist\Fino-<version>-win-x64.zip` (~97 MB): `FinoSetup.exe`, the
self-contained API, the built console, the database as SQL, `Setup.bat` and the maintenance
scripts.

**One zip covers both ways of hosting it.** Earlier releases also produced a `-separate` zip that
was the same build re-arranged into `api\` + `ui\` for IIS — a second 56 MB download to express
one difference (the panel beside the API instead of inside it). That is gone: `Setup.bat` asks how
it should be hosted, and choosing *two IIS sites* hands over to `Configure-Split.ps1`, which copies
this same `app\` folder into the two site folders and writes both configuration files with matching
keys. `-AlsoSeparate` still emits the old layout for a client who is following the old instructions.

To build the client's Android app into the release as well:

```powershell
pwsh .\deploy\Build-Release.ps1 -Version 2026.09.20.5 -AndroidClient finocore
```

That runs the Gradle build (needs a JDK and the Android SDK on the build machine) and puts the APK
in `android\` inside the zip; the setup then publishes it to `<portal>/app/`. Without a JDK it says
so and carries on. An APK built elsewhere can be dropped into `deploy\prebuilt-apk\<id>.apk` and is
picked up from there.

The zip is large because two self-contained .NET applications are in it (the API and the setup
window), each carrying its own runtime. That is the trade: about 40 MB more to download once per
client, against a whole class of "it will not start, some runtime is missing" on their server.

The build also **blanks every credential** out of the shipped `appsettings.json` and fails the
build if any is left — a forgotten default must not travel from one client to the next. It also
rejects any script containing a non-ASCII character, because Windows PowerShell 5.1 fails to parse
those on a server with a different code page.

### When entities change

The schema ships as SQL generated from the EF model (the migration chain in this repo has drifted
from the entities):

```powershell
pwsh .\deploy\tools\New-SchemaScript.ps1     # regenerates Scripts\install\10_schema.sql
```

Commit the result. **Never edit a script that has already shipped** — an installed database records
its hash and refuses to re-run it. Ship changes as a new, higher-numbered file
(`46_patch_whatever.sql`).

---

## 6. Day two

| Task | How |
|---|---|
| Update | `FinoSetup.exe` from the new release → **Update to this release** |
| Change logo or name | `FinoSetup.exe` → **Change logo / name** |
| Backup | Automatic, nightly at 02:00. On demand: `FinoSetup.exe` → **Back up now** |
| Restore check | `RESTORE VERIFYONLY FROM DISK = '...bak'` — do it once, for real |
| Logs | Event Viewer → Windows Logs → Application |
| Is it up? | `https://…/health` |

Backups go to `C:\FinoBackups`: database, uploads, and the configuration file with the keys.
**Copy them off the server.** A backup on the same machine does not survive that machine, and a
restored database without the key file cannot be used — every session and the panel's encrypted
transport stop matching.

---

## 7. Per-client keys (the part that matters for reselling)

Every installation gets its own JWT signing secret and AES key/IV. A token minted on client A's
server is not valid on client B's. The installer writes both ends — API config and panel
`config.json` — in one step, which is why they cannot drift apart.

The values in the shipped `appsettings.json` are blanked at build time, so there is no shared
default to leak.

---

## 8. What is in this folder

```
deploy/
  gui/FinoSetup/          the window the customer sees; published as FinoSetup.exe
    MainForm.cs           the screens
    ScriptRunner.cs       runs Setup.ps1 and streams its output back
    UiKit.cs              the theme, in one place
  Setup.bat / Setup.ps1   the same install, from a console
  Install-Fino.ps1        the installer Setup calls
  Update-Fino.ps1         upgrade with backup + automatic rollback
  Backup-Fino.ps1         database + uploads + keys
  Set-Branding.ps1        change the client's name/logo on a live installation
  Configure-Split.ps1     IIS hosting: lays the build out as two sites and configures both
  Build-Release.ps1       make the customer zip (you)
  INSTALL.txt             plain-language instructions that ship in the zip
  SPLIT-DEPLOY.txt        the same, for the two-IIS-site layout
  ANDROID-AND-PUSH.txt    the phone app, and what push actually needs
  ENTERPRISE-ROADMAP.md   what to build next, and why
  prebuilt-apk/           drop <client>.apk here to ship it without a Gradle build
  lib/
    FinoCommon.ps1        prompts, secrets, SQL, service control, install state, versions,
                          and the copy that lays one build out into two IIS site folders
    Prerequisites.ps1     server checks; SQL Server Express download and silent install
    Branding.ps1          logo handling (PNG/JPG/SVG), favicon generation, config.json,
                          the palette read off the logo, app-config.json, the download page
  tools/                  developer tools: schema generator, website-content export
  docker/                 the Linux path
  optional-seeds/         provider seeds (Pay1, JaiJui) - only for clients using them
```

---

## 9. Verified, and not

**Run end to end on a Windows Server 2019 box:** fresh install (including logo, service, database
build, first login through the panel), upgrade between two versions with backup and version
tracking, re-branding a live installation, the nightly backup task (registered, run, and the
resulting `.bak` verified restorable), and detection of an existing IIS deployment.

**FinoSetup.exe:** every screen was rendered and reviewed (`FinoSetup.exe --screenshot <folder>`),
and the seam that matters — the exe running the scripts and reading back what they report — was
run on this server (`FinoSetup.exe --selftest`), as was a complete install through the exe's own
answer-file path, ending in a branded, working panel.

**Not exercised yet:**

* **The SQL Server Express download and silent install** — the test machine already had SQL
  Server, so that path is written but unproven. Try it once on a clean VM before sending someone
  to a client site with it.
* **The IIS → service migration**, for the same reason in reverse: the only IIS deployment
  available was a live one, and testing a migration against it was not worth the risk. The code
  backs up first and asks before touching anything, but it has not been run.
* **The Docker path** (`deploy/docker/`) — no Docker host available.
* **Clicking through the window by hand.** The screens were rendered and the logic behind each
  button was run directly, but nobody has sat in front of it and pressed the buttons in order. Do
  that once on a spare machine before handing it to a client.

---

## 10. Known limits

* **A fresh database takes 3–5 minutes** to build on SQL Express (236 tables, 208 indexes). That is
  the wait, not a hang. The installer shows progress and `/health` reports `starting`.
* **Passwords are SHA-256, unsalted.** Not adequate for a payments platform — see
  `ENTERPRISE-ROADMAP.md`, item 1. No installer can fix that; the login path has to change.
* **One instance only.** The wallet lock and the caches are in-process, so two instances against
  one database can double-spend. Roadmap item 4.
* `Cors:AllowedOrigins` still defaults to `*` in the shipped template so that an existing hand-made
  deployment does not break on upgrade. Installs written by this installer always list the real
  origins.
* Provider credentials (Pay1, JaiJui, recharge aggregators) are not part of an install — each
  client signs their own contracts and enters them in *Settings → API Providers*.
