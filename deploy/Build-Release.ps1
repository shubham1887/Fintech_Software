<#
.SYNOPSIS
    Builds a self-contained Fino release that a non-technical person can install.

.DESCRIPTION
    Run this on a DEVELOPER machine (the one with .NET and Node installed). It produces a single
    zip that contains everything a customer's server needs, for BOTH ways of hosting it:

        Fino-<version>-win-x64.zip
          app\                 the API, published self-contained (no .NET install on the server)
            wwwroot\panel\     the operator console, built and served by the same process
            Scripts\install\   the database, as SQL; applied automatically on first start
          Install-Fino.ps1     the wizard the customer runs (Windows service hosting)
          Configure-Split.ps1  the same build laid out as two IIS sites, and configured
          iis\ui.web.config    the panel's IIS rules (SPA routing, .apk MIME type)
          android\             the client's APK, published to the portal by the installer
          INSTALL.txt          five lines, for the person clicking

    There used to be a second "-separate" zip carrying a pre-split copy of the same build. It
    doubled the download to express one difference - the panel beside the API instead of inside it
    - so the split is now made on the customer's server by Configure-Split.ps1, from the one copy.

    Self-contained on purpose: the commonest failed install on a customer's Windows Server is a
    missing or mismatched .NET runtime, and "the zip already contains the runtime" removes it.

.EXAMPLE
    pwsh ./deploy/Build-Release.ps1 -Version 2026.09.1
#>
[CmdletBinding()]
param(
    # Stamped into the release name and into version.json inside the build.
    [string] $Version = (Get-Date -Format 'yyyy.MM.dd.HHmm'),
    # Skip the web build when you have just built it and only changed backend code.
    [switch] $SkipWeb,
    # Also emit the old pre-split folder (api\ + ui\) and its own zip. Only needed for a client
    # who is still following the two-zip instructions; one package now covers both layouts.
    [switch] $AlsoSeparate,
    # Build this client's Android APK into the release, so the installer can publish it without
    # anybody copying files by hand. The id is the one in Fino_Android\whitelabel\clients.json
    # (tools\new-client.ps1 adds it). Needs a JDK and the Android SDK on THIS machine - the APK is
    # baked with the client's portal address, so it cannot be built on the customer's server.
    [string] $AndroidClient,
    # win-x64 covers Windows Server 2016+. linux-x64 is for the Docker path.
    [ValidateSet('win-x64', 'linux-x64')]
    [string] $Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

<#
    Runs a command-line tool quietly.

    npm and dotnet both write progress and warnings to stderr. Windows PowerShell turns anything a
    native program writes to stderr into a red "NativeCommandError" block, so a build that succeeded
    with two vite warnings looks exactly like a build that failed - which is alarming and wastes
    time. Routing through cmd merges the streams, so the output is just text: kept quiet on success,
    printed in full on failure.
#>
function Invoke-Tool {
    param(
        [Parameter(Mandatory = $true)] [string] $Command,
        [string] $WorkingDirectory,
        [string] $What = 'The command'
    )

    $previous = $null
    if ($WorkingDirectory) { $previous = Get-Location; Set-Location $WorkingDirectory }
    try {
        $output = & cmd /c "$Command 2>&1"
        $exitCode = $LASTEXITCODE
    } finally {
        if ($previous) { Set-Location $previous }
    }

    if ($exitCode -ne 0) {
        Write-Host ''
        Write-Host "$What failed (exit code $exitCode). Full output:" -ForegroundColor Red
        $output | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
        throw "$What failed with exit code $exitCode."
    }

    return $output
}
$repoRoot   = (Resolve-Path (Join-Path $scriptRoot '..')).Path
$apiProject = Join-Path $repoRoot 'Fino_API\FintechApp.csproj'
$webRoot    = Join-Path $repoRoot 'Fino_Web'
$staging    = Join-Path $repoRoot "deploy\dist\Fino-$Version-$Runtime"
$zipPath    = Join-Path $repoRoot "deploy\dist\Fino-$Version-$Runtime.zip"

function Step([string] $text) { Write-Host "`n=== $text" -ForegroundColor Cyan }

Step "Fino release $Version ($Runtime)"

if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging -Force | Out-Null
$appDir = Join-Path $staging 'app'

# ---------------------------------------------------------------------------
# 1. API - self-contained publish.
# ---------------------------------------------------------------------------
Step 'Publishing the API'
Write-Host '  (a minute or two)' -ForegroundColor DarkGray
Invoke-Tool -What 'Publishing the API' -Command (
    'dotnet publish "{0}" -c Release -r {1} --self-contained true -p:PublishSingleFile=false -p:DebugType=none -o "{2}" --nologo -v quiet' `
        -f $apiProject, $Runtime, $appDir) | Out-Null
Write-Host '  API published' -ForegroundColor Green

# appsettings.json ships as the template of last resort. The real configuration is written by the
# installer into appsettings.Production.json, which overrides it - so the development connection
# string and keys in the template are never what a customer runs with. Blank them anyway: a
# shipped secret is a shipped secret, and every customer must get their own.
$templatePath = Join-Path $appDir 'appsettings.json'
$template = Get-Content $templatePath -Raw

# Blanked with a text substitution rather than by parsing: appsettings.json contains // comments,
# which .NET's configuration reader accepts and Windows PowerShell's ConvertFrom-Json does not -
# and those comments are the documentation for the settings, so they have to survive.
$secretKeys = @(
    'DefaultConnection',   # database credentials
    'SecretKey',           # JWT signing key
    'Key', 'IV',           # AES transport key/IV
    'ServerKey', 'WebServerKey',
    'MemberId', 'Password' # provider credentials
)
foreach ($key in $secretKeys) {
    $template = [regex]::Replace($template, "(""$key""\s*:\s*)""[^""]*""", '$1""')
}

# A value left behind here would ship to every client, so fail the build rather than warn.
$leaked = [regex]::Matches($template, '("(?:' + ($secretKeys -join '|') + ')"\s*:\s*)"([^"]+)"')
if ($leaked.Count -gt 0) {
    throw ("appsettings.json still contains {0} secret value(s) after sanitising - do not ship this build." -f $leaked.Count)
}
[System.IO.File]::WriteAllText($templatePath, $template, [System.Text.UTF8Encoding]::new($false))
Write-Host '  appsettings.json template blanked of credentials'

# A development-only override file must never reach a customer's server.
Remove-Item (Join-Path $appDir 'appsettings.Development.json') -ErrorAction SilentlyContinue

$installScripts = Get-ChildItem (Join-Path $appDir 'Scripts\install') -Filter '*.sql' -ErrorAction SilentlyContinue
if (-not $installScripts) { throw 'Scripts\install\*.sql did not reach the publish output - the database installer would have nothing to run.' }
Write-Host ("  database scripts included: {0}" -f $installScripts.Count)

# ---------------------------------------------------------------------------
# 2. Web - built and placed where the API serves it.
# ---------------------------------------------------------------------------
$panelDir = Join-Path $appDir 'wwwroot\panel'
if (-not $SkipWeb) {
    Step 'Building the operator console'
    try {
        if (-not (Test-Path (Join-Path $webRoot 'node_modules'))) {
            Write-Host '  installing npm packages (first time only, a few minutes)' -ForegroundColor DarkGray
            Invoke-Tool -What 'npm ci' -WorkingDirectory $webRoot -Command 'npm ci' | Out-Null
        }
        # Baked-in default is the same origin, because the panel is served by the API. The
        # installer still writes config.json explicitly; this only decides what happens if that
        # file is ever deleted.
        $env:VITE_API_URL = '/api/'
        Invoke-Tool -What 'npm run build' -WorkingDirectory $webRoot -Command 'npm run build' | Out-Null
        Write-Host '  console built' -ForegroundColor Green
    } finally {
        Remove-Item Env:\VITE_API_URL -ErrorAction SilentlyContinue
    }
}

$webDist = Join-Path $webRoot 'dist'
if (-not (Test-Path (Join-Path $webDist 'index.html'))) {
    throw "No build found at $webDist. Run without -SkipWeb."
}
New-Item -ItemType Directory -Path $panelDir -Force | Out-Null
Copy-Item (Join-Path $webDist '*') $panelDir -Recurse -Force

# ---------------------------------------------------------------------------
# 3. The graphical setup.
#
# FinoSetup.exe is what the customer is told to double-click. It collects the answers and then runs
# the very same Setup.ps1 the command line uses, so there is one implementation of the install and
# two ways to reach it. Published self-contained for the same reason the API is: a server with no
# .NET runtime must still be able to run it.
# ---------------------------------------------------------------------------
Step 'Building the graphical setup'
$guiProject = Join-Path $scriptRoot 'gui\FinoSetup\FinoSetup.csproj'
if (Test-Path $guiProject) {
    $guiOutput = Join-Path ([System.IO.Path]::GetTempPath()) ("fino-gui-{0}" -f [guid]::NewGuid().ToString('N'))
    Invoke-Tool -What 'Building FinoSetup.exe' -Command (
        'dotnet publish "{0}" -c Release -r win-x64 --self-contained true -o "{1}" --nologo -v quiet' `
            -f $guiProject, $guiOutput) | Out-Null

    $exe = Join-Path $guiOutput 'FinoSetup.exe'
    if (-not (Test-Path $exe)) { throw 'FinoSetup.exe was not produced.' }
    Copy-Item $exe $staging -Force

    # A single-file publish still leaves the .pdb and a couple of loose files behind; only the exe
    # travels.
    Remove-Item $guiOutput -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host ("  FinoSetup.exe: {0} MB" -f [math]::Round((Get-Item (Join-Path $staging 'FinoSetup.exe')).Length / 1MB, 1))
} else {
    Write-Host '  gui\FinoSetup not found - packaging the command-line setup only.' -ForegroundColor Yellow
}

# ---------------------------------------------------------------------------
# 3b. The Android app.
#
# The APK is baked with the client's portal address, so it is a per-client artefact and it has to
# be built here, where the Android SDK is - not on the customer's server. Building it into the
# release is what lets the installer publish it automatically: without this step somebody has to
# remember to copy an APK onto the server afterwards, and that is exactly the manual step this kit
# exists to remove.
# ---------------------------------------------------------------------------
if ($AndroidClient) {
    Step "Building the Android app ($AndroidClient)"

    $androidRoot = Join-Path $repoRoot 'Fino_Android'
    $gradlew = Join-Path $androidRoot 'gradlew.bat'
    $registry = Join-Path $androidRoot 'whitelabel\clients.json'

    if (-not (Test-Path $gradlew)) {
        throw "Fino_Android\gradlew.bat not found - cannot build the app."
    }

    $clients = (Get-Content $registry -Raw | ConvertFrom-Json).clients
    if (-not ($clients | Where-Object { $_.id -eq $AndroidClient })) {
        throw ("'{0}' is not in clients.json. Add it first:`n    .\Fino_Android\tools\new-client.ps1 -Id {0} -Name ... -ApplicationId ... -Url ..." -f $AndroidClient)
    }

    # A JDK is what actually decides whether this can run here. Checked up front so the failure is
    # one clear sentence rather than a Gradle stack trace.
    $javaOk = $false
    try { & cmd /c "java -version 2>&1" | Out-Null; $javaOk = ($LASTEXITCODE -eq 0) } catch { }
    if (-not $javaOk -and $env:JAVA_HOME -and (Test-Path (Join-Path $env:JAVA_HOME 'bin\java.exe'))) { $javaOk = $true }

    # Gradle finds the Android SDK through local.properties, or ANDROID_HOME / ANDROID_SDK_ROOT.
    # When only the variable is set, write the file - otherwise the build fails with "SDK location
    # not found", which reads like a missing SDK rather than a missing line in a file.
    $localProperties = Join-Path $androidRoot 'local.properties'
    $sdkRoot = if ($env:ANDROID_HOME) { $env:ANDROID_HOME } else { $env:ANDROID_SDK_ROOT }
    if (-not (Test-Path $localProperties) -and $sdkRoot -and (Test-Path $sdkRoot)) {
        [System.IO.File]::WriteAllText($localProperties, ("sdk.dir={0}`n" -f $sdkRoot.Replace('\', '\\')))
        Write-Host "  local.properties written (sdk.dir = $sdkRoot)" -ForegroundColor DarkGray
    }
    $sdkOk = (Test-Path $localProperties) -or ($sdkRoot -and (Test-Path $sdkRoot))

    if (-not ($javaOk -and $sdkOk)) {
        $missing = if (-not $javaOk) { 'No JDK' } else { 'No Android SDK' }
        Write-Host "  $missing on this machine, so the APK cannot be built here." -ForegroundColor Yellow
        Write-Host '  Build it on a machine with Android Studio and drop it in:' -ForegroundColor Yellow
        Write-Host ("      .\Fino_Android\gradlew.bat assemble{0}Release" -f (Get-Culture).TextInfo.ToTitleCase($AndroidClient)) -ForegroundColor DarkGray
        Write-Host ("      copy the APK to  deploy\prebuilt-apk\{0}.apk  and build the release again" -f $AndroidClient) -ForegroundColor DarkGray
    } else {
        $flavour = (Get-Culture).TextInfo.ToTitleCase($AndroidClient)
        Invoke-Tool -What "Building the $AndroidClient APK" -WorkingDirectory $androidRoot `
                    -Command ".\gradlew.bat assemble${flavour}Release --console=plain -q" | Out-Null

        $built = Get-ChildItem (Join-Path $androidRoot "app\build\outputs\apk\$AndroidClient\release") `
                     -Filter '*.apk' -ErrorAction SilentlyContinue |
                 Where-Object { $_.Name -notlike '*unsigned*' } | Select-Object -First 1

        if (-not $built) {
            Write-Host '  Gradle produced no signed APK. A release build needs a signing key -' -ForegroundColor Yellow
            Write-Host '  see Fino_Android\README.md. The release is still usable; the app just is not in it.' -ForegroundColor Yellow
        } else {
            New-Item -ItemType Directory -Path (Join-Path $staging 'android') -Force | Out-Null
            Copy-Item $built.FullName (Join-Path $staging "android\$AndroidClient.apk") -Force
            Write-Host ("  {0}.apk  ({1} MB) - the installer will publish it automatically" -f `
                $AndroidClient, [math]::Round($built.Length / 1MB, 1)) -ForegroundColor Green
        }
    }
}

# A pre-built APK dropped here by hand (built on a machine with Android Studio) is picked up too,
# so the "build it elsewhere" path still ends with an automatic install.
#
# A file just built by Gradle above WINS. This used to be a plain -Force copy, so a leftover in
# prebuilt-apk\ silently overwrote the APK this run had just produced and the release shipped an
# older app than the one it was built from - with nothing in the output saying so. The APK is
# baked with the client's portal address, so a stale one is not a cosmetic problem.
$prebuiltFolder = Join-Path $scriptRoot 'prebuilt-apk'
if (Test-Path $prebuiltFolder) {
    $prebuilt = Get-ChildItem $prebuiltFolder -Filter '*.apk' -ErrorAction SilentlyContinue
    if ($prebuilt) {
        New-Item -ItemType Directory -Path (Join-Path $staging 'android') -Force | Out-Null
        foreach ($apk in $prebuilt) {
            $destination = Join-Path $staging "android\$($apk.Name)"
            if (Test-Path $destination) {
                Write-Host ("  prebuilt-apk\{0} ignored - this run built that app itself." -f $apk.Name) -ForegroundColor Yellow
                Write-Host '    Delete the stale file, or leave it: the freshly built APK is the one shipped.' -ForegroundColor DarkGray
                continue
            }
            Copy-Item $apk.FullName $destination -Force
            Write-Host ("  pre-built app included: {0} ({1} MB)" -f $apk.Name, [math]::Round($apk.Length / 1MB, 1)) -ForegroundColor Green
        }
    }
}

Step 'Packaging'

# Setup.bat is the only file the customer is told to run; everything else is what it calls, or
# what is needed afterwards (backup, re-branding).
$shipped = @(
    'Setup.bat',
    'Setup.ps1',
    'Install-Fino.ps1',
    'Update-Fino.ps1',
    'Backup-Fino.ps1',
    'Set-Branding.ps1',
    # The IIS route out of the same package: lays this build out as two sites and configures both.
    'Configure-Split.ps1',
    'INSTALL.txt',
    'SPLIT-DEPLOY.txt',
    'ANDROID-AND-PUSH.txt'
)
foreach ($file in $shipped) {
    $source = Join-Path $scriptRoot $file
    if (-not (Test-Path $source)) { throw "Missing from the deploy folder: $file" }
    Copy-Item $source $staging -Force
}

Copy-Item (Join-Path $scriptRoot 'lib') (Join-Path $staging 'lib') -Recurse -Force

# ---------------------------------------------------------------------------
# The panel's IIS rules, shipped as a file so the split deployment can drop it into the UI site.
#
# IIS serves index.html for paths that are not files: /reports and /users/12 are React Router
# routes, not folders. Needs the URL Rewrite module, which any server already hosting a
# single-page app will have.
# ---------------------------------------------------------------------------
$uiWebConfig = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Single page app" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <!-- A request for something with a file extension is a FILE, not a route. If it is
                 missing it must 404. Without this condition every missing file - an APK that has
                 not been uploaded yet, an old asset - comes back as index.html with status 200,
                 and a browser happily saves that HTML under the requested name. That is how a
                 downloaded .apk turns out "corrupt": it is a web page wearing an .apk extension. -->
            <add input="{REQUEST_URI}" pattern="\.[a-zA-Z0-9]{1,8}(\?.*)?$" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <remove fileExtension=".svg" />
      <mimeMap fileExtension=".svg" mimeType="image/svg+xml" />
      <remove fileExtension=".woff2" />
      <mimeMap fileExtension=".woff2" mimeType="font/woff2" />
      <!-- Without this IIS answers 404 for the Android app: it refuses to serve a file whose
           extension it has no MIME type for, and .apk is not one it knows. -->
      <remove fileExtension=".apk" />
      <mimeMap fileExtension=".apk" mimeType="application/vnd.android.package-archive" />
    </staticContent>
    <!-- The bundle is ~1 MB of JS/CSS before compression and nothing in it is pre-compressed,
         so without this every first visit downloads the lot uncompressed. -->
    <urlCompression doStaticCompression="true" doDynamicCompression="true" />
    <httpProtocol>
      <customHeaders>
        <!-- index.html and config.json must not be cached, or a client keeps running the previous
             release and the previous API address after an update. This is the SITE-WIDE default;
             assets/ overrides it in the <location> below. -->
        <add name="Cache-Control" value="no-cache" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
  <!-- The hashed bundle, cached hard.
       This block is why it exists: a customHeaders rule in <system.webServer> above applies to
       EVERY response from the site, assets/ included. The "no-cache" default therefore told the
       browser to re-validate every chunk on every single navigation - a dozen-plus extra round
       trips before the page can paint, which on a phone is the difference between "instant" and
       "sometimes it just doesn't load". Vite already fingerprints these filenames (index-DxrG..js),
       so the file at a given URL never changes and a year is safe; a new release ships new names
       and index.html is still uncached, so an update is picked up immediately. -->
  <location path="assets">
    <system.webServer>
      <httpProtocol>
        <customHeaders>
          <remove name="Cache-Control" />
          <add name="Cache-Control" value="public, max-age=31536000, immutable" />
        </customHeaders>
      </httpProtocol>
    </system.webServer>
  </location>
</configuration>
'@
$iisDir = Join-Path $staging 'iis'
New-Item -ItemType Directory -Path $iisDir -Force | Out-Null
[System.IO.File]::WriteAllText((Join-Path $iisDir 'ui.web.config'), $uiWebConfig, [System.Text.UTF8Encoding]::new($false))

if (Test-Path (Join-Path $scriptRoot 'optional-seeds')) {
    Copy-Item (Join-Path $scriptRoot 'optional-seeds') (Join-Path $staging 'optional-seeds') -Recurse -Force
}

# A .ps1 with a non-ASCII character in it is read as ANSI by Windows PowerShell 5.1 on a server
# with a different code page, and the file fails to parse. Catching it here is cheaper than
# catching it on a customer's machine.
foreach ($script in (Get-ChildItem $staging -Filter '*.ps1' -Recurse)) {
    $text = [System.IO.File]::ReadAllText($script.FullName)
    $offenders = ([char[]]$text | Where-Object { [int]$_ -gt 126 })
    if ($offenders.Count -gt 0) {
        throw ("{0} contains {1} non-ASCII character(s) - Windows PowerShell 5.1 will fail to parse it on some servers." -f $script.Name, $offenders.Count)
    }
}

@{
    version   = $Version
    runtime   = $Runtime
    builtOn   = (Get-Date).ToString('o')
    builtBy   = "$env:USERNAME@$env:COMPUTERNAME"
} | ConvertTo-Json | Set-Content (Join-Path $appDir 'version.json') -Encoding UTF8

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zipPath -CompressionLevel Optimal

# ---------------------------------------------------------------------------
# 5. The same build, split into two folders for IIS.
#
# The combined service is the better way to deploy this, but plenty of clients already run the API
# and the panel as two IIS sites on two hostnames, each with its own certificate - and asking them
# to change their DNS and certificates to take an update is not reasonable. So every release also
# produces the classic two-folder layout.
#
# The two differ from the combined build in exactly two ways: the panel is not inside the API's
# wwwroot, and the panel's config.json points at the API's hostname instead of at the same origin.
# ---------------------------------------------------------------------------
if ($AlsoSeparate) {
    Step 'Building the separate UI / API folders (for IIS)'

    $separate = Join-Path $repoRoot "deploy\dist\Fino-$Version-separate"
    if (Test-Path $separate) { Remove-Item $separate -Recurse -Force }
    $separateApi = Join-Path $separate 'api'
    $separateUi  = Join-Path $separate 'ui'
    New-Item -ItemType Directory -Path $separateApi, $separateUi -Force | Out-Null

    Copy-Item (Join-Path $appDir '*') $separateApi -Recurse -Force

    # The panel travels in its own folder here, so it must not also sit inside the API's wwwroot:
    # two copies would drift the moment one of them is re-branded.
    Remove-Item (Join-Path $separateApi 'wwwroot\panel') -Recurse -Force -ErrorAction SilentlyContinue

    Copy-Item (Join-Path $webDist '*') $separateUi -Recurse -Force

    [System.IO.File]::WriteAllText((Join-Path $separateUi 'web.config'), $uiWebConfig, [System.Text.UTF8Encoding]::new($false))

    Copy-Item (Join-Path $scriptRoot 'Configure-Split.ps1') $separate -Force
    Copy-Item (Join-Path $scriptRoot 'Backup-Fino.ps1') $separate -Force
    Copy-Item (Join-Path $scriptRoot 'lib') (Join-Path $separate 'lib') -Recurse -Force
    Copy-Item (Join-Path $scriptRoot 'SPLIT-DEPLOY.txt') (Join-Path $separate 'READ-ME-FIRST.txt') -Force
    Copy-Item (Join-Path $scriptRoot 'ANDROID-AND-PUSH.txt') $separate -Force

    # The Android app travels with this layout too - Configure-Split publishes it into ui\app\.
    $androidStaged = Join-Path $staging 'android'
    if (Test-Path $androidStaged) {
        Copy-Item $androidStaged (Join-Path $separate 'android') -Recurse -Force
    }

    $separateZip = "$separate.zip"
    if (Test-Path $separateZip) { Remove-Item $separateZip -Force }
    Compress-Archive -Path (Join-Path $separate '*') -DestinationPath $separateZip -CompressionLevel Optimal

    $apiMb = [math]::Round((Get-ChildItem $separateApi -Recurse -File | Measure-Object Length -Sum).Sum / 1MB, 1)
    $uiMb  = [math]::Round((Get-ChildItem $separateUi  -Recurse -File | Measure-Object Length -Sum).Sum / 1MB, 1)
    Write-Host ("  api\ {0} MB   ui\ {1} MB" -f $apiMb, $uiMb)
}

$sizeMb = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)

Write-Host ''
Write-Host '  ============================================================' -ForegroundColor Green
Write-Host '    Release ready' -ForegroundColor Green
Write-Host '  ============================================================' -ForegroundColor Green
Write-Host ''
Write-Host '  The zip to send the client:' -ForegroundColor White
Write-Host "      $zipPath" -ForegroundColor Yellow
Write-Host "      $sizeMb MB"
Write-Host ''
Write-Host '  The same thing already unzipped, to test on this machine:' -ForegroundColor White
Write-Host "      $staging" -ForegroundColor Yellow
Write-Host '      -> double-click FinoSetup.exe in that folder'
Write-Host ''
Write-Host '  What is inside:' -ForegroundColor White
foreach ($item in (Get-ChildItem $staging | Sort-Object { -not $_.PSIsContainer }, Name)) {
    $label = if ($item.PSIsContainer) { "$($item.Name)\" } else { $item.Name }
    $note = switch -Wildcard ($label) {
        'FinoSetup.exe'    { 'the window the client runs  <-- START HERE' }
        'Setup.bat'        { 'the same install, without the window' }
        'app\'             { 'the application, its runtime and the database scripts' }
        'lib\'             { 'shared PowerShell used by the scripts' }
        'iis\'             { 'the panel web.config used by the two-site (IIS) layout' }
        'android\'         { 'the client APK, published to the portal during setup' }
        'Configure-Split.ps1' { 'IIS hosting: lays this build out as two sites and configures them' }
        'INSTALL.txt'      { 'instructions for whoever installs it' }
        'SPLIT-DEPLOY.txt' { 'instructions for the two-site (IIS) layout' }
        'optional-seeds\'  { 'provider seeds - only if the client uses Pay1 / JaiJui' }
        default            { '' }
    }
    Write-Host ("      {0,-22} {1}" -f $label, $note) -ForegroundColor $(if ($label -eq 'FinoSetup.exe') { 'Yellow' } else { 'Gray' })
}
Write-Host ''
Write-Host '  On the client server: unzip the WHOLE folder, then double-click FinoSetup.exe.' -ForegroundColor DarkGray
Write-Host '  The same zip installs on a new server and updates one that already has Fino.' -ForegroundColor DarkGray
Write-Host ''
Write-Host '  Client who hosts UI and API as two IIS sites? Same zip - run Configure-Split.ps1:' -ForegroundColor DarkGray
Write-Host '  it copies this build into the two site folders and writes both config files.' -ForegroundColor DarkGray

if ($AlsoSeparate) {
    Write-Host ''
    Write-Host '  ------------------------------------------------------------' -ForegroundColor DarkGray
    Write-Host '  For a client who runs UI and API as two IIS sites:' -ForegroundColor White
    Write-Host "      $separate" -ForegroundColor Yellow
    Write-Host "      $separate.zip"
    Write-Host '          api\                 -> the api.client.com site'
    Write-Host '          ui\                  -> the ui.client.com site'
    Write-Host '          Configure-Split.ps1  -> writes both config files, with matching keys'
    Write-Host '          READ-ME-FIRST.txt    -> the steps'
    Write-Host ''
    Write-Host '  Send a client ONE of the two: the exe for a new server, these folders when' -ForegroundColor DarkGray
    Write-Host '  they already have the two IIS sites and certificates.' -ForegroundColor DarkGray
}
Write-Host ''
