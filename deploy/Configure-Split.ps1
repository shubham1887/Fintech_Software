<#
.SYNOPSIS
    Writes the two configuration files for a split (IIS) deployment: one for the API site, one for
    the UI site.

.DESCRIPTION
    Use this with the "separate" build, where the API and the operator console are hosted as two
    IIS sites on two hostnames - the layout most existing clients already have
    (api.client.com + ui.client.com, each with its own certificate).

    It writes:
      api\appsettings.Production.json  - connection string, this installation's own JWT secret and
                                         AES key/IV, and the UI's origin in the CORS list
      ui\config.json                   - the brand, the logo, the API address, and the SAME AES
                                         key/IV as the API

    Those two files have to agree about the AES key or nothing can log in, which is exactly why
    they are written together by one script instead of by hand in two places.

    No Kestrel port is written: under IIS the port comes from the site binding, so there is no port
    to get wrong here.

.EXAMPLE
    .\Configure-Split.ps1 -ApiUrl "https://api.client.com" -UiUrl "https://ui.client.com" `
        -SqlServer ".\SQLEXPRESS" -DatabaseName "ClientDb" `
        -BrandName "Client" -AdminMobile "9876543210" -AdminPassword "Str0ng#Pass"
#>
[CmdletBinding()]
param(
    [string] $ApiUrl,
    [string] $UiUrl,
    [string] $SqlServer,
    [string] $DatabaseName,
    [string] $SqlUser,
    [string] $SqlPassword,
    [string] $BrandName,
    [string] $LogoPath,
    # The square artwork the phone app is built around. Shown inside the app instead of the wide
    # logo, and it is the same file the launcher icon is generated from.
    [string] $AppIconPath,
    [string] $AdminName = 'Administrator',
    [string] $AdminMobile,
    [string] $AdminEmail,
    [string] $AdminPassword,
    # An existing database that already has its users: skip creating a first administrator.
    [switch] $ExistingDatabase,
    [string] $ApiFolder,
    [string] $UiFolder,
    # The IIS application pool the api site runs under. With Windows authentication this is the
    # account SQL Server sees, so it is the account that needs rights on the database. The default
    # matches an app pool named after the site, which is what IIS creates.
    [string] $ApiAppPool,
    # Push notifications. The SERVER needs the service-account JSON (Firebase Console -> Project
    # settings -> Service accounts -> Generate new private key); the ANDROID APP needs the four
    # values from google-services.json. Leave them out and everything still works - alerts show in
    # the in-app bell, they just do not reach the phone's notification tray.
    [string] $FcmServiceAccountFile,
    [string] $FcmProjectId,
    [string] $FcmAppId,
    [string] $FcmApiKey,
    [string] $FcmSenderId,
    # Support details the phone app shows.
    [string] $SupportWhatsApp,
    [string] $SupportEmail,
    [string] $SupportPhone
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $scriptRoot 'lib\FinoCommon.ps1')
# Loaded here rather than inside the logo branch: the Android app's config file and the download
# page are written on every run, logo or no logo.
. (Join-Path $scriptRoot 'lib\Branding.ps1')

Write-Banner 'Fino' 'configuring a split (IIS) deployment'

# ---------------------------------------------------------------------------
# Where the two sites live.
#
# The release used to ship a second, pre-split copy of itself (a "-separate" zip) purely so that
# these two folders already existed. That doubled the download for one difference: the panel sits
# beside the API instead of inside it. One package now carries one copy of the build, and this
# step lays it out into the two IIS folders - which also removes the "copy the files into the site
# folder" step people were doing by hand.
# ---------------------------------------------------------------------------
$packagedApp = Join-Path $scriptRoot 'app'      # the API publish, with the panel in wwwroot\panel
$legacyApi   = Join-Path $scriptRoot 'api'      # the old pre-split layout, still supported
$legacyUi    = Join-Path $scriptRoot 'ui'

if (Test-Path $packagedApp) {
    # This is the one-package release: the build is here and has to be laid out into the two site
    # folders, whether or not the person already knows where they are.
    Write-Step 'Site folders'
    Write-Host '  The two folders IIS serves these sites from. They are created if missing.' -ForegroundColor DarkGray
    if (-not $ApiFolder) { $ApiFolder = Ask '  API site folder  ' -Default 'C:\inetpub\wwwroot\api.client.com' -Required }
    if (-not $UiFolder)  { $UiFolder  = Ask '  Panel site folder' -Default 'C:\inetpub\wwwroot\ui.client.com' -Required }

    Copy-PackageToSites -PackagedApp $packagedApp -ApiFolder $ApiFolder -UiFolder $UiFolder -ScriptRoot $scriptRoot
} else {
    # The older pre-split layout, where the files are already arranged as api\ and ui\ beside this
    # script and only the configuration is missing.
    if (-not $ApiFolder) { $ApiFolder = $legacyApi }
    if (-not $UiFolder)  { $UiFolder  = $legacyUi }
}

foreach ($folder in @($ApiFolder, $UiFolder)) {
    if (-not (Test-Path $folder)) { Write-Fail "Folder not found: $folder"; exit 1 }
}

# Keep both pools warm and compress from the first hit. Best-effort; see the function's own notes
# for why IIS's defaults make the first visit of every quiet spell feel broken.
Write-Step 'IIS performance defaults'
Set-IisSitePerformance -Folders @($ApiFolder, $UiFolder) -AppPools @($ApiAppPool)

# ---------------------------------------------------------------------------
# Questions
# ---------------------------------------------------------------------------
Write-Step 'Addresses'
Write-Host '  Exactly as they will be typed in a browser, including https://' -ForegroundColor DarkGray
if (-not $ApiUrl) { $ApiUrl = Ask '  API site address   (e.g. https://api.client.com)' -Required }
if (-not $UiUrl)  { $UiUrl  = Ask '  Panel site address (e.g. https://ui.client.com)' -Required }
$ApiUrl = $ApiUrl.TrimEnd('/')
$UiUrl  = $UiUrl.TrimEnd('/')

Write-Step 'Database'
if (-not $SqlServer)    { $SqlServer    = Ask '  SQL Server instance' -Default 'localhost\SQLEXPRESS' -Required }
if (-not $DatabaseName) { $DatabaseName = Ask '  Database name' -Required }
if (-not $SqlUser -and -not $ExistingDatabase) {
    if (-not (Ask-YesNo '  Connect with Windows authentication?' $true)) {
        $SqlUser     = Ask '  SQL login name' -Default 'sa' -Required
        $SqlPassword = Ask '  SQL login password' -Required -Secret
    }
}

$auth = if ($SqlUser) { "User ID=$SqlUser;Password=$SqlPassword" } else { 'Integrated Security=True' }
$connectionString = "Server=$SqlServer;Database=$DatabaseName;$auth;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Command Timeout=0;"

$test = Test-SqlConnection ("Server=$SqlServer;Database=master;$auth;Encrypt=False;TrustServerCertificate=True;Connection Timeout=15;")
if ($test.ok) { Write-Ok "Connected: $($test.version)" }
else { Write-Warn2 "Could not reach SQL Server now: $($test.error)"; Write-Warn2 'The configuration will still be written - fix the connection before starting the site.' }

# ---------------------------------------------------------------------------
# With Windows authentication, SQL Server sees the IIS application pool identity - not whoever is
# running this script, and not the machine account. That identity has no login by default, so the
# site starts and then answers "Login failed for user 'IIS APPPOOL\...'" forever. Granting it here
# is the IIS equivalent of what the service installer does for the service account.
# ---------------------------------------------------------------------------
if (-not $SqlUser -and $test.ok) {
    Write-Step 'Granting the IIS application pool access to the database'

    if (-not $ApiAppPool) {
        # IIS names the pool after the site by default, and the site is named after the API host.
        # Ask falls back to that default on its own when there is no console to ask.
        $ApiAppPool = Ask '  Application pool name of the API site' -Default ([Uri]$ApiUrl).Host -Required
    }

    $account = "IIS APPPOOL\$ApiAppPool"
    $masterConn = "Server=$SqlServer;Database=master;$auth;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
    $dbConn     = "Server=$SqlServer;Database=$DatabaseName;$auth;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"

    try {
        # CREATE LOGIN cannot take the name as a parameter, so it is quoted with QUOTENAME into a
        # variable and run through sp_executesql.
        Invoke-Sql $masterConn @"
DECLARE @sql nvarchar(max);
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @account)
BEGIN
    SET @sql = N'CREATE LOGIN ' + QUOTENAME(@account) + N' FROM WINDOWS';
    EXEC sp_executesql @sql;
END
"@ @{ '@account' = $account } | Out-Null

        # The database may not exist yet - the API creates it on first start. In that case the
        # grant happens against master only and is repeated by re-running this script afterwards.
        $dbExists = Invoke-Sql $masterConn 'SELECT database_id FROM sys.databases WHERE name = @name' @{ '@name' = $DatabaseName }
        if ($null -ne $dbExists -and $dbExists -isnot [DBNull]) {
            Invoke-Sql $dbConn @"
DECLARE @sql nvarchar(max);
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @account)
BEGIN
    SET @sql = N'CREATE USER ' + QUOTENAME(@account) + N' FOR LOGIN ' + QUOTENAME(@account);
    EXEC sp_executesql @sql;
END
SET @sql = N'ALTER ROLE [db_owner] ADD MEMBER ' + QUOTENAME(@account);
EXEC sp_executesql @sql;
"@ @{ '@account' = $account } | Out-Null
            Write-Ok "'$account' granted db_owner on [$DatabaseName]"
        } else {
            Write-Ok "'$account' given a SQL login"
            Write-Warn2 "[$DatabaseName] does not exist yet - the API creates it on first start."
            Write-Warn2 'Run this script again afterwards so the app pool gets rights on it.'
        }
    } catch {
        Write-Fail "Could not grant the application pool access: $($_.Exception.Message)"
        Write-Host '  Do it by hand in SQL Server, then recycle the app pool:' -ForegroundColor Yellow
        Write-Host "    CREATE LOGIN [$account] FROM WINDOWS;" -ForegroundColor DarkGray
        Write-Host "    USE [$DatabaseName]; CREATE USER [$account] FOR LOGIN [$account];" -ForegroundColor DarkGray
        Write-Host "    ALTER ROLE [db_owner] ADD MEMBER [$account];" -ForegroundColor DarkGray
    }
}

Write-Step 'Branding'
if (-not $BrandName) { $BrandName = Ask '  Company / brand name' -Required }

if (-not $ExistingDatabase -and -not $AdminMobile) {
    Write-Step "The client's owner login"
    Write-Host '  Leave the mobile blank if this database already has its users.' -ForegroundColor DarkGray
    $AdminMobile = Ask '  Mobile number'
    if ($AdminMobile) {
        if (-not $AdminEmail) { $AdminEmail = Ask '  Email address' -Default "admin@$(($BrandName -replace '[^A-Za-z0-9]','').ToLower()).local" }
        while (-not $AdminPassword) {
            $AdminPassword = Ask '  Password (at least 8 characters)' -Required -Secret
            $again         = Ask '  Password again' -Required -Secret
            if ($AdminPassword -ne $again)   { Write-Warn2 'The two passwords do not match.'; $AdminPassword = $null; continue }
            if ($AdminPassword.Length -lt 8) { Write-Warn2 'Too short.'; $AdminPassword = $null }
        }
    }
}

# ---------------------------------------------------------------------------
# Keys - generated once, written to both sides.
# ---------------------------------------------------------------------------
Write-Step "Generating this installation's keys"

$apiConfigPath = Join-Path $ApiFolder 'appsettings.Production.json'
$uiConfigPath  = Join-Path $UiFolder 'config.json'

if (Test-Path $apiConfigPath) {
    $existing  = Get-Content $apiConfigPath -Raw | ConvertFrom-Json
    $jwtSecret = $existing.JwtSettings.SecretKey
    $aesKey    = $existing.Encryption.Key
    $aesIv     = $existing.Encryption.IV
    Write-Ok 'Reusing the keys already in this folder'
} else {
    $jwtSecret = New-Secret 64
    $aesKey    = New-Secret 32
    $aesIv     = New-Secret 16
    Write-Ok 'New JWT secret and AES key/IV created for this client'
}

# ---------------------------------------------------------------------------
# API configuration
# ---------------------------------------------------------------------------
Write-Step 'Writing the API configuration'

$apiSettings = [ordered]@{
    ConnectionStrings = [ordered]@{ DefaultConnection = $connectionString }
    JwtSettings       = [ordered]@{ SecretKey = $jwtSecret }
    Encryption        = [ordered]@{ Key = $aesKey; IV = $aesIv }
    # The panel is a different origin in this layout, so it has to be listed explicitly. The
    # browser sends the session on these calls; an open list would let any site an operator visits
    # drive the panel as them.
    Cors              = [ordered]@{ AllowedOrigins = @($UiUrl) }
    # The panel is served by IIS from its own site here, not by this process.
    Panel             = [ordered]@{ ServeStaticFiles = $false }
    Database          = [ordered]@{ AutoSetup = $true; SeedAdmin = (-not $ExistingDatabase -and [bool]$AdminMobile) }
    Bootstrap         = [ordered]@{
        BrandName     = $BrandName
        AdminName     = $AdminName
        AdminMobile   = $AdminMobile
        AdminEmail    = $AdminEmail
        AdminPassword = $AdminPassword
    }
    Logging           = [ordered]@{ LogLevel = [ordered]@{ Default = 'Information'; 'Microsoft.AspNetCore' = 'Warning' } }
}

# Push. The service account is copied next to the configuration rather than referenced where it
# was downloaded, so a later move of the folder does not silently turn push off.
if ($FcmServiceAccountFile) {
    if (-not (Test-Path $FcmServiceAccountFile)) {
        Write-Warn2 "Firebase service-account file not found: $FcmServiceAccountFile"
        Write-Warn2 'Push will stay off; alerts will still appear in the in-app bell.'
    } else {
        $destination = Join-Path $ApiFolder 'firebase-service-account.json'
        Copy-Item $FcmServiceAccountFile $destination -Force
        try {
            $acl = Get-Acl $destination
            $acl.SetAccessRuleProtection($true, $false)
            foreach ($identity in @('NT AUTHORITY\SYSTEM', 'BUILTIN\Administrators', 'IIS_IUSRS')) {
                try { $acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($identity, 'Read', 'Allow'))) } catch { }
            }
            Set-Acl $destination $acl
        } catch { }

        $apiSettings['FCM'] = [ordered]@{ ServiceAccountFile = $destination }
        if ($FcmProjectId) { $apiSettings['FCM']['ProjectId'] = $FcmProjectId }
        Write-Ok 'Firebase service account installed - push to phones is on'
    }
}

$apiSettings | ConvertTo-Json -Depth 20 | Set-Content $apiConfigPath -Encoding UTF8
Write-Ok (Split-Path -Leaf $apiConfigPath)

# Only the site's application-pool identity and administrators should be able to read the file that
# holds the database credentials and the signing keys.
try {
    $acl = Get-Acl $apiConfigPath
    $acl.SetAccessRuleProtection($true, $false)
    foreach ($identity in @('NT AUTHORITY\SYSTEM', 'BUILTIN\Administrators', 'IIS_IUSRS')) {
        try {
            $acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($identity, 'Read', 'Allow')))
        } catch { }
    }
    $acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule('BUILTIN\Administrators', 'FullControl', 'Allow')))
    Set-Acl $apiConfigPath $acl
    Write-Ok 'Configuration locked down (Administrators, SYSTEM, IIS_IUSRS read)'
} catch {
    Write-Warn2 "Could not tighten the file permissions: $($_.Exception.Message)"
}

# ---------------------------------------------------------------------------
# UI configuration
# ---------------------------------------------------------------------------
Write-Step 'Writing the panel configuration'

$uiConfig = [ordered]@{}
if (Test-Path $uiConfigPath) {
    try {
        $current = Get-Content $uiConfigPath -Raw | ConvertFrom-Json
        foreach ($property in $current.PSObject.Properties) { $uiConfig[$property.Name] = $property.Value }
    } catch { }
}

$uiConfig['appName']       = $BrandName
$uiConfig['tagline']       = 'Console'
$uiConfig['theme']         = if ($uiConfig.Contains('theme') -and $uiConfig['theme']) { $uiConfig['theme'] } else { 'azure' }
$uiConfig['loginHeadline'] = "The control room for your $BrandName network."
$uiConfig['loginSubtext']  = 'Recharges, money transfer, AEPS, wallets and settlements - one secure console for every role in your distribution chain.'
# The API is on its own hostname here, so this cannot be left as a relative path.
$uiConfig['apiUrl']        = "$ApiUrl/api/"
$uiConfig['encKey']        = $aesKey
$uiConfig['encIv']         = $aesIv
if (-not $uiConfig.Contains('logoUrl')) { $uiConfig['logoUrl'] = '' }

# Logos are uploaded through the API in this layout, so they are served from the API's host.
if ($LogoPath) {
    if (-not (Test-Path $LogoPath)) {
        Write-Warn2 "Logo file not found: $LogoPath"
    } else {
        $brandingFolder = Join-Path $ApiFolder 'wwwroot\uploads\branding'
        New-Item -ItemType Directory -Path $brandingFolder -Force | Out-Null

        $extension = [System.IO.Path]::GetExtension($LogoPath).ToLower()
        if ($extension -eq '.svg') {
            Copy-Item $LogoPath (Join-Path $brandingFolder 'logo.svg') -Force
            Copy-Item $LogoPath (Join-Path $UiFolder 'favicon.svg') -Force
            $uiConfig['logoUrl'] = "$ApiUrl/uploads/branding/logo.svg"
        } else {
            $logoPng    = Join-Path $brandingFolder 'logo.png'
            $faviconPng = Join-Path $brandingFolder 'favicon-64.png'
            Resize-LogoImage -SourcePath $LogoPath -DestinationPath $logoPng    -Size 512
            Resize-LogoImage -SourcePath $LogoPath -DestinationPath $faviconPng -Size 64 -Square
            New-FaviconSvg -PngPath $faviconPng -DestinationPath (Join-Path $UiFolder 'favicon.svg')
            New-FaviconIco -PngPath $faviconPng -DestinationPath (Join-Path $UiFolder 'favicon.ico')
            $uiConfig['logoUrl'] = "$ApiUrl/uploads/branding/logo.png"

            # Paint the portal in the client's own colours instead of the stock blue. Nobody
            # sends a hex code with their logo, so it is read off the artwork.
            $palette = Get-LogoPalette -Path $logoPng
            if ($palette) {
                $uiConfig['brandColor']  = $palette.Primary
                $uiConfig['navbarColor'] = $palette.Navbar
                $uiConfig['appbarColor'] = $palette.Appbar
                # Not read by the panel - carried here so the Android app gets the same accent.
                $uiConfig['accentColor'] = $palette.Accent
                Write-Ok "Portal colours taken from the logo (brand $($palette.Primary), sidebar $($palette.Navbar))"
            }
        }
        Copy-Item $LogoPath (Join-Path $brandingFolder ('logo-original' + $extension)) -Force
        Write-Ok 'Logo prepared (served from the API host, shown by the panel)'
    }
}

# The square app artwork, kept exactly as supplied - its margin and rounded corners are the design.
if ($AppIconPath) {
    if (-not (Test-Path $AppIconPath)) {
        Write-Warn2 "App icon file not found: $AppIconPath"
    } else {
        $brandingFolder = Join-Path $ApiFolder 'wwwroot\uploads\branding'
        New-Item -ItemType Directory -Path $brandingFolder -Force | Out-Null
        if ([System.IO.Path]::GetExtension($AppIconPath).ToLower() -eq '.svg') {
            Copy-Item $AppIconPath (Join-Path $brandingFolder 'app-icon.svg') -Force
            $uiConfig['appIconUrl'] = "$ApiUrl/uploads/branding/app-icon.svg"
        } else {
            Resize-LogoImage -SourcePath $AppIconPath -DestinationPath (Join-Path $brandingFolder 'app-icon.png') -Size 512 -NoTrim
            $uiConfig['appIconUrl'] = "$ApiUrl/uploads/branding/app-icon.png"
        }
        Write-Ok 'App icon prepared (shown inside the phone app)'
    }
}

# No BOM: the browser fetches this and parses it as JSON.
[System.IO.File]::WriteAllText($uiConfigPath, ($uiConfig | ConvertTo-Json -Depth 10), [System.Text.UTF8Encoding]::new($false))
Write-Ok (Split-Path -Leaf $uiConfigPath)

# The Android app's own configuration, in the same folder. Without this the phone app opens
# whichever portal the panel build happened to ship with - somebody else's.
$appConfigPath = Write-AndroidAppConfig -UiFolder $UiFolder -BrandName $BrandName -PortalUrl $UiUrl `
    -SupportWhatsApp $SupportWhatsApp -SupportEmail $SupportEmail -SupportPhone $SupportPhone `
    -ColorPrimary ([string]$uiConfig['brandColor']) -ColorAccent ([string]$uiConfig['accentColor']) `
    -ColorSplash ([string]$uiConfig['navbarColor']) `
    -FcmProjectId $FcmProjectId -FcmAppId $FcmAppId -FcmApiKey $FcmApiKey -FcmSenderId $FcmSenderId
Write-Ok ((Split-Path -Leaf $appConfigPath) + "  (Android app reads this on every launch)")

# The page people are sent to in order to install the app.
$downloadPage = Write-AppDownloadPage -UiFolder $UiFolder -BrandName $BrandName -PortalUrl $UiUrl `
    -LogoUrl $uiConfig["logoUrl"] -ApkSource (Join-Path $scriptRoot 'android')
Write-Ok ("app/index.html  ->  $UiUrl/app/  (the download page)")

# ---------------------------------------------------------------------------
# What is left to do
# ---------------------------------------------------------------------------
Write-Host ''
Write-Host '  ============================================================' -ForegroundColor Green
Write-Host '    Both configuration files written' -ForegroundColor Green
Write-Host '  ============================================================' -ForegroundColor Green
Write-Host ''
Write-Host '  API site  ->' -ForegroundColor White -NoNewline; Write-Host "  $ApiFolder"
Write-Host '  Panel site->' -ForegroundColor White -NoNewline; Write-Host "  $UiFolder"
Write-Host ''
Write-Host '  In IIS:' -ForegroundColor White
Write-Host "    1. Point the $ApiUrl site at the api folder."
Write-Host '       Its application pool must be set to "No Managed Code".'
Write-Host "    2. Point the $UiUrl site at the ui folder."
Write-Host '    3. Start both sites, then open the panel address.'
Write-Host ''
Write-Host '  The database builds itself the first time the API site is hit - the first request' -ForegroundColor DarkGray
Write-Host '  after a fresh install takes a few minutes and answers 503 until it is ready.' -ForegroundColor DarkGray
Write-Host ''
Write-Warn2 'Keep appsettings.Production.json safe. It holds this installation''s keys, and a restored'
Write-Warn2 'database without them cannot be used.'
exit 0
