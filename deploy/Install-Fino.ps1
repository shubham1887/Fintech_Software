<#
.SYNOPSIS
    Installs Fino on a Windows server: database, API and operator console, as one Windows service.

.DESCRIPTION
    Run this from the unzipped release folder, as Administrator. It asks a short list of questions,
    then does everything else:

      * checks that it can reach SQL Server with the details given
      * generates this installation's own JWT secret and AES key/IV (never shared between clients)
      * copies the application to the install folder
      * writes appsettings.Production.json and the panel's config.json
      * registers and starts the "Fino API" Windows service, and opens the firewall port
      * the service creates the database, applies the schema and seeds it on first start
      * waits for /health, then removes the first-login password from the config file

    Re-running it against an existing installation upgrades the files and leaves the database,
    uploads and the existing administrator alone - but Update-Fino.ps1 is the better path for that,
    because it takes a backup first.

.EXAMPLE
    # Interactive (what the customer does)
    .\Install-Fino.ps1

.EXAMPLE
    # Unattended, for your own repeat installs
    .\Install-Fino.ps1 -Unattended -BrandName "RiPay" -SqlServer "localhost\SQLEXPRESS" `
        -DatabaseName "RiPay" -AdminMobile "9876543210" -AdminPassword "Str0ng#Pass" `
        -PublicUrl "https://panel.ripay.co.in"
#>
[CmdletBinding()]
param(
    [string] $BrandName,
    [string] $InstallPath = 'C:\Fino',
    [int]    $Port = 8080,
    [string] $SqlServer,
    [string] $DatabaseName = 'FinoApp',
    # Blank SqlUser = Windows authentication (the service account must then have rights on SQL).
    [string] $SqlUser,
    [string] $SqlPassword,
    [string] $AdminName = 'Administrator',
    [string] $AdminMobile,
    [string] $AdminEmail,
    [string] $AdminPassword,
    # The address operators will type, e.g. https://panel.client.com. Used for CORS and for the
    # printed summary. Leave blank for an intranet install reached by IP.
    [string] $PublicUrl,
    # Optional TLS: a .pfx and its password make the service serve HTTPS directly.
    [string] $CertificatePath,
    [string] $CertificatePassword,
    [string] $ServiceName = 'FinoAPI',
    # The Windows account the service runs as. LocalSystem by default; with Windows authentication
    # this is also the account SQL Server sees, and the installer grants it rights on the database.
    # A domain account (DOMAIN\svc-fino) is the tidier choice where one exists.
    [string] $ServiceAccount = 'NT AUTHORITY\SYSTEM',
    [string] $ServiceAccountPassword,
    # The client's logo: PNG, JPG or SVG. Resized for the panel, and turned into the browser-tab
    # icon. Can be left out and added later with Set-Branding.ps1.
    [string] $LogoPath,
    # The square artwork the phone app is built around (its launcher tile). Optional: without it
    # the app shows the wide logo, same as the website.
    [string] $AppIconPath,
    # Keys carried over from an existing deployment being migrated. Supplying them keeps every
    # signed-in operator signed in and keeps the panel's encrypted transport working; leaving them
    # out generates fresh ones, which is what a brand-new client should get.
    [string] $JwtSecret,
    [string] $AesKey,
    [string] $AesIv,
    # For a migration onto an existing database: do not ask for or create a first administrator,
    # because that database already has its people in it.
    [switch] $SkipAdminSeed,
    [switch] $Unattended
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# Shared helpers - prompts, secrets, SQL, service control, install state. Kept in one place so the
# installer, the updater and the branding tool cannot drift apart.
. (Join-Path $scriptRoot 'lib\FinoCommon.ps1')
. (Join-Path $scriptRoot 'lib\Branding.ps1')

# ---------------------------------------------------------------------------
# 0. Prerequisites
# ---------------------------------------------------------------------------
Write-Host @"

  ============================================================
    Fino  -  installation
  ============================================================
"@ -ForegroundColor White

Write-Step 'Checking the server'

$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
           ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Fail 'This window is not running as Administrator.'
    Write-Host '        Close it, right-click Install.bat and choose "Run as administrator".' -ForegroundColor Yellow
    exit 1
}
Write-Ok 'Running as Administrator'

$appSource = Join-Path $scriptRoot 'app'
if (-not (Test-Path (Join-Path $appSource 'FintechApp.exe'))) {
    Write-Fail "The application folder is missing: $appSource"
    Write-Host '        Unzip the whole release and run Install.bat from inside the unzipped folder.' -ForegroundColor Yellow
    exit 1
}
Write-Ok 'Application files found'

$portInUse = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

# ---------------------------------------------------------------------------
# 1. Questions
# ---------------------------------------------------------------------------
if (-not $Unattended) {
    Write-Step 'A few questions'
    Write-Host '  (press Enter to accept the value in brackets)' -ForegroundColor DarkGray
    Write-Host ''

    if (-not $BrandName)    { $BrandName    = Ask 'Company / brand name shown in the panel' -Default 'Fino' }
    if (-not $SqlServer)    { $SqlServer    = Ask 'SQL Server instance' -Default 'localhost\SQLEXPRESS' -Required }
    if (-not $DatabaseName) { $DatabaseName = Ask 'Database name (created if it does not exist)' -Default 'FinoApp' -Required }

    if (-not $SqlUser) {
        $useWindowsAuth = (Ask 'Connect to SQL with Windows authentication? (y/n)' -Default 'y').ToLower().StartsWith('y')
        if (-not $useWindowsAuth) {
            $SqlUser     = Ask 'SQL login name' -Default 'sa' -Required
            $SqlPassword = Ask 'SQL login password' -Required -Secret
        }
    }

    $InstallPath = Ask 'Install folder' -Default $InstallPath -Required
    $Port        = [int](Ask 'Port for the panel and API' -Default "$Port" -Required)
    $PublicUrl   = Ask 'Public address operators will use (blank if only by IP)' -Default $PublicUrl

    if (-not $LogoPath) { $LogoPath = Read-LogoPath -AllowSkip }

    if (-not $SkipAdminSeed) {
        Write-Host ''
        Write-Host '  First administrator login:' -ForegroundColor White
        if (-not $AdminMobile) { $AdminMobile = Ask '  Mobile number' -Required }
        if (-not $AdminEmail)  { $AdminEmail  = Ask '  Email address' -Default "admin@$($BrandName.ToLower() -replace '[^a-z0-9]','').local" }
        if (-not $AdminPassword) {
            while ($true) {
                $AdminPassword = Ask '  Password (min 8 characters)' -Required -Secret
                $again         = Ask '  Password again' -Required -Secret
                if ($AdminPassword -ne $again)      { Write-Warn2 'The two passwords do not match.'; continue }
                if ($AdminPassword.Length -lt 8)    { Write-Warn2 'Too short - use at least 8 characters.'; continue }
                break
            }
        }
    }
}

$required = @(
    @{ n = 'SQL Server';    v = $SqlServer },
    @{ n = 'Database name'; v = $DatabaseName }
)
if (-not $SkipAdminSeed) {
    $required += @{ n = 'Admin mobile';   v = $AdminMobile }
    $required += @{ n = 'Admin password'; v = $AdminPassword }
}
foreach ($item in $required) {
    if (-not $item.v) { Write-Fail "$($item.n) is required."; exit 1 }
}

# The port is checked before anything is written to disk. A configuration pointing at a port the
# service cannot bind is one that fails on every start, including after a rollback - so it must
# never be written in the first place.
$portCheck = Test-InstallPort -Port $Port -IgnoreServiceName $ServiceName
if (-not $portCheck.ok) {
    Write-Fail $portCheck.reason
    Write-Host '  Nothing has been changed. Run the setup again with a different port.' -ForegroundColor Yellow
    exit 1
}
if (-not $BrandName) { $BrandName = 'Fino' }

# ---------------------------------------------------------------------------
# 2. Database reachability - checked before anything is written to disk.
# ---------------------------------------------------------------------------
Write-Step 'Testing the database connection'

$auth = if ($SqlUser) { "User ID=$SqlUser;Password=$SqlPassword" } else { 'Integrated Security=True' }
$connectionString = "Server=$SqlServer;Database=$DatabaseName;$auth;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Command Timeout=0;"
# The application's own database may not exist yet, so the reachability test goes to master.
$masterTest = Test-SqlConnection ("Server=$SqlServer;Database=master;$auth;Encrypt=False;TrustServerCertificate=True;Connection Timeout=15;")

if (-not $masterTest.ok) {
    Write-Fail "Could not connect to SQL Server at '$SqlServer'."
    Write-Host "         $($masterTest.error)" -ForegroundColor DarkGray
    Write-Host ''
    Write-Host '  Common causes:' -ForegroundColor Yellow
    Write-Host '    - the instance name is wrong (try  localhost\SQLEXPRESS  or just  localhost )'
    Write-Host '    - SQL Server service is stopped (services.msc -> SQL Server)'
    Write-Host '    - TCP/IP is disabled in SQL Server Configuration Manager'
    Write-Host '    - the login or password is wrong'
    exit 1
}
Write-Ok "Connected: $($masterTest.version)"

# ---------------------------------------------------------------------------
# 2b. Create the database and grant the SERVICE account rights on it.
#
# This is done here, by the administrator running the installer, rather than left to the service:
# the service then never needs CREATE DATABASE on master, which is a permission a payments service
# has no business holding. It is also the difference between a clear failure now and a service
# that installs "successfully" and then cannot start.
# ---------------------------------------------------------------------------
Write-Step 'Preparing the database'

$masterConn = "Server=$SqlServer;Database=master;$auth;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"

try {
    $exists = Invoke-Sql $masterConn 'SELECT database_id FROM sys.databases WHERE name = @name' @{ '@name' = $DatabaseName }
    if ($null -eq $exists -or $exists -is [DBNull]) {
        # The name cannot be a parameter in CREATE DATABASE; it comes from our own prompt, and the
        # brackets are escaped so a stray "]" cannot break out of the identifier.
        Invoke-Sql $masterConn ("CREATE DATABASE [{0}]" -f ($DatabaseName -replace '\]', ']]')) | Out-Null
        Write-Ok "Database [$DatabaseName] created"
    } else {
        Write-Ok "Database [$DatabaseName] already exists - it will be upgraded, not replaced"
    }
} catch {
    Write-Fail "Could not create the database: $($_.Exception.Message)"
    Write-Host '  The account you are running as needs permission to create a database on this SQL Server.' -ForegroundColor Yellow
    exit 1
}

if (-not $SqlUser) {
    # Windows authentication: the connection string carries no credentials, so SQL sees whichever
    # account the SERVICE runs as - LocalSystem (NT AUTHORITY\SYSTEM) unless told otherwise. That
    # account needs a login and ownership of this one database, and nothing else.
    $dbConn = "Server=$SqlServer;Database=$DatabaseName;$auth;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
    $account = $ServiceAccount
    try {
        # CREATE LOGIN / CREATE USER cannot take the name as a parameter, so it is quoted with
        # QUOTENAME into a variable and run through sp_executesql. (EXEC('...' + QUOTENAME(@x))
        # is a syntax error - EXEC concatenates variables and literals, not function results.)
        Invoke-Sql $masterConn @"
DECLARE @sql nvarchar(max);
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @account)
BEGIN
    SET @sql = N'CREATE LOGIN ' + QUOTENAME(@account) + N' FROM WINDOWS';
    EXEC sp_executesql @sql;
END
"@ @{ '@account' = $account } | Out-Null

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

        Write-Ok "'$account' (the service account) granted db_owner on [$DatabaseName]"
    } catch {
        Write-Fail "Could not grant the service account access: $($_.Exception.Message)"
        Write-Host "  Grant it by hand, then re-run:" -ForegroundColor Yellow
        Write-Host "    CREATE LOGIN [$account] FROM WINDOWS;" -ForegroundColor DarkGray
        Write-Host "    USE [$DatabaseName]; CREATE USER [$account] FOR LOGIN [$account];" -ForegroundColor DarkGray
        Write-Host "    ALTER ROLE [db_owner] ADD MEMBER [$account];" -ForegroundColor DarkGray
        exit 1
    }
}

# ---------------------------------------------------------------------------
# 3. Secrets - one set per installation.
# ---------------------------------------------------------------------------
Write-Step 'Generating this installation''s keys'

$configDir  = Join-Path $InstallPath 'app'
$prodConfig = Join-Path $configDir 'appsettings.Production.json'

if (Test-Path $prodConfig) {
    # An upgrade: the existing keys must survive, or every operator is logged out and - worse -
    # the panel's AES key stops matching the API's and nothing can log in at all.
    $existing  = Get-Content $prodConfig -Raw | ConvertFrom-Json
    $jwtSecret = $existing.JwtSettings.SecretKey
    $aesKey    = $existing.Encryption.Key
    $aesIv     = $existing.Encryption.IV
    Write-Ok 'Reusing the keys already on this server (upgrade)'
} elseif ($JwtSecret -and $AesKey -and $AesIv) {
    # A migration from an older deployment: same reasoning, except the keys come from the
    # configuration of the thing being replaced.
    $jwtSecret = $JwtSecret
    $aesKey    = $AesKey
    $aesIv     = $AesIv
    Write-Ok 'Carrying over the keys from the existing deployment (sessions stay valid)'
} else {
    $jwtSecret = New-Secret 64
    $aesKey    = New-Secret 32   # AES-256 key, as 32 ASCII characters
    $aesIv     = New-Secret 16   # AES block IV, as 16 ASCII characters
    Write-Ok 'New JWT secret and AES key/IV created for this client'
}

# ---------------------------------------------------------------------------
# 4. Files
# ---------------------------------------------------------------------------
Write-Step "Copying the application to $InstallPath"

if ($existingService) {
    Write-Host '  Stopping the running service first...'
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    # A stopping service can hold its files for a moment; copying too early fails with "in use".
    $waited = 0
    while ((Get-Service -Name $ServiceName).Status -ne 'Stopped' -and $waited -lt 30) {
        Start-Sleep -Seconds 1; $waited++
    }
}

New-Item -ItemType Directory -Path $configDir -Force | Out-Null

# Uploads (KYC documents, operator/service icons) live inside the app folder and must never be
# overwritten by a release; the same goes for the configuration written below.
$preserve = @('appsettings.Production.json', 'wwwroot\uploads')
$backupTemp = Join-Path $env:TEMP "fino-preserve-$(Get-Random)"
foreach ($rel in $preserve) {
    $src = Join-Path $configDir $rel
    if (Test-Path $src) {
        $dst = Join-Path $backupTemp $rel
        New-Item -ItemType Directory -Path (Split-Path -Parent $dst) -Force | Out-Null
        Copy-Item $src $dst -Recurse -Force
    }
}

Copy-Item (Join-Path $appSource '*') $configDir -Recurse -Force

if (Test-Path $backupTemp) {
    foreach ($rel in $preserve) {
        $src = Join-Path $backupTemp $rel
        if (Test-Path $src) { Copy-Item $src (Join-Path $configDir $rel) -Recurse -Force }
    }
    Remove-Item $backupTemp -Recurse -Force
}
Write-Ok 'Files in place'

# ---------------------------------------------------------------------------
# 5. Configuration
# ---------------------------------------------------------------------------
Write-Step 'Writing the configuration'

$scheme  = if ($CertificatePath) { 'https' } else { 'http' }
$origins = @()
if ($PublicUrl) { $origins += $PublicUrl.TrimEnd('/') }
$origins += "${scheme}://localhost:$Port"

$settings = [ordered]@{
    ConnectionStrings = [ordered]@{ DefaultConnection = $connectionString }
    JwtSettings       = [ordered]@{ SecretKey = $jwtSecret }
    Encryption        = [ordered]@{ Key = $aesKey; IV = $aesIv }
    # The panel is served by this same service, so its origin is the service's own.
    Cors              = [ordered]@{ AllowedOrigins = $origins }
    Panel             = [ordered]@{ ServeStaticFiles = $true }
    # SeedAdmin is off for a migration: that database already has its administrators, and the
    # seeder only ever acts on a database with none, but saying so explicitly is clearer than
    # relying on that.
    Database          = [ordered]@{ AutoSetup = $true; SeedAdmin = (-not $SkipAdminSeed) }
    Bootstrap         = [ordered]@{
        BrandName     = $BrandName
        AdminName     = $AdminName
        AdminMobile   = if ($SkipAdminSeed) { '' } else { $AdminMobile }
        AdminEmail    = if ($SkipAdminSeed) { '' } else { $AdminEmail }
        # Removed from this file once the first start has used it (see step 8).
        AdminPassword = if ($SkipAdminSeed) { '' } else { $AdminPassword }
    }
    Kestrel           = [ordered]@{ Endpoints = [ordered]@{} }
    Logging           = [ordered]@{ LogLevel = [ordered]@{ Default = 'Information'; 'Microsoft.AspNetCore' = 'Warning' } }
}

if ($CertificatePath) {
    $settings.Kestrel.Endpoints['Https'] = [ordered]@{
        Url         = "https://0.0.0.0:$Port"
        Certificate = [ordered]@{ Path = $CertificatePath; Password = $CertificatePassword }
    }
} else {
    $settings.Kestrel.Endpoints['Http'] = [ordered]@{ Url = "http://0.0.0.0:$Port" }
}

$settings | ConvertTo-Json -Depth 20 | Set-Content $prodConfig -Encoding UTF8
Write-Ok 'appsettings.Production.json written'

# Only the service account and administrators should be able to read the file that holds the
# database password and the signing keys.
$acl = Get-Acl $prodConfig
$acl.SetAccessRuleProtection($true, $false)
$configReaders = @('NT AUTHORITY\SYSTEM', 'BUILTIN\Administrators')
# The service has to be able to read its own configuration, so a non-default service account is
# added here too - otherwise the lockdown silently stops the service from starting.
if ($ServiceAccount -and $configReaders -notcontains $ServiceAccount) { $configReaders += $ServiceAccount }
foreach ($identity in $configReaders) {
    $acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($identity, 'FullControl', 'Allow')))
}
Set-Acl $prodConfig $acl
Write-Ok ("Configuration file readable only by {0}" -f ($configReaders -join ', '))

# The panel reads config.json in the browser at load time: the client's name, their logo and the
# transport keys. That is what makes re-selling this build practical - a new client is a new
# config.json, not a new build.
try {
    # The Android app is pointed at this same address: in this hosting the panel and the API are
    # one origin, so the portal the phone opens is the panel's own address.
    $panelAddressForApp = if ($PublicUrl) { $PublicUrl.TrimEnd('/') } else { "${scheme}://$($env:COMPUTERNAME):$Port" }
    $branding = Set-FinoBranding -InstallPath $InstallPath -BrandName $BrandName -LogoPath $LogoPath -AppIconPath $AppIconPath `
                                 -ApiUrl '/api/' -EncKey $aesKey -EncIv $aesIv `
                                 -PublicUrl $panelAddressForApp `
                                 -ApkSource (Join-Path $scriptRoot 'android')
    Write-Ok "Panel branded as '$BrandName'"
} catch {
    # A bad logo file must not cost the whole installation: the platform works with the name shown
    # as text, and Set-Branding.ps1 can add the logo afterwards.
    Write-Warn2 "Branding could not be fully applied: $($_.Exception.Message)"
    Write-Warn2 'Finish the install, then run Set-Branding.ps1 with a PNG or SVG logo.'
}

# ---------------------------------------------------------------------------
# 6. Service + firewall
# ---------------------------------------------------------------------------
Write-Step 'Registering the Windows service'

$exePath = Join-Path $configDir 'FintechApp.exe'

if ($existingService) {
    & sc.exe config $ServiceName binPath= "`"$exePath`"" start= auto | Out-Null
    Write-Ok "Service '$ServiceName' updated"
} else {
    & sc.exe create $ServiceName binPath= "`"$exePath`"" start= auto DisplayName= "$BrandName API" | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Fail "Could not create the service (sc.exe returned $LASTEXITCODE)."; exit 1 }
    & sc.exe description $ServiceName "$BrandName fintech platform - API and operator console." | Out-Null
    Write-Ok "Service '$ServiceName' created"
}

# Run under the requested account. LocalSystem is the default and needs no password; anything else
# is a real account whose password sc.exe stores in the service configuration.
if ($ServiceAccount -and $ServiceAccount -ne 'NT AUTHORITY\SYSTEM') {
    if ($ServiceAccountPassword) {
        & sc.exe config $ServiceName obj= "$ServiceAccount" password= "$ServiceAccountPassword" | Out-Null
    } else {
        & sc.exe config $ServiceName obj= "$ServiceAccount" | Out-Null
    }
    if ($LASTEXITCODE -ne 0) { Write-Fail "Could not set the service account to '$ServiceAccount'."; exit 1 }
    Write-Ok "Service runs as $ServiceAccount"

    # That account also has to be able to read the install folder and write uploads.
    $acl = Get-Acl $configDir
    $acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule(
        $ServiceAccount, 'Modify', 'ContainerInherit,ObjectInherit', 'None', 'Allow')))
    Set-Acl $configDir $acl
}

# Restart on failure: after 30s, again after 60s, and reset the counter daily. Without this a
# transient SQL outage at boot leaves the platform down until somebody notices.
& sc.exe failure $ServiceName reset= 86400 actions= restart/30000/restart/60000/restart/120000 | Out-Null

# ASPNETCORE_ENVIRONMENT has to be set for the service itself; a user-level variable is not seen.
$svcKey = "HKLM:\SYSTEM\CurrentControlSet\Services\$ServiceName"
Set-ItemProperty -Path $svcKey -Name 'Environment' -Value @('ASPNETCORE_ENVIRONMENT=Production') -Type MultiString
Write-Ok 'Service environment set to Production'

$ruleName = "$BrandName Panel (TCP $Port)"
if (-not (Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Protocol TCP -LocalPort $Port -Action Allow -Profile Any | Out-Null
}
Write-Ok "Firewall opened for port $Port"

if ($portInUse -and -not $existingService) {
    Write-Warn2 "Port $Port was already in use by another program before this install - if the service fails to start, that is why."
}

# ---------------------------------------------------------------------------
# 7. Start, and wait for the database to build itself.
# ---------------------------------------------------------------------------
Write-Step 'Starting the service'
Write-Host '  The first start creates the database and can take 3-5 minutes. Please wait.' -ForegroundColor DarkGray

# Retried rather than called once: the Service Control Manager is not always ready to start a
# service in the same second it was created and had its environment written, and the failure looks
# like a broken install when it is only impatience.
if (-not (Start-FinoService -ServiceName $ServiceName)) {
    Write-Fail "The service '$ServiceName' would not start."
    Write-Host '  Check Event Viewer -> Windows Logs -> Application for the reason.' -ForegroundColor Yellow
    exit 1
}

$healthUrl = "${scheme}://localhost:$Port/health"
$health = Wait-FinoHealthy -HealthUrl $healthUrl -ServiceName $ServiceName -TimeoutMinutes 12

if (-not $health.ok) {
    switch ($health.reason) {
        'setup-failed' {
            Write-Fail 'The database setup failed.'
            Write-Host "  $($health.error)" -ForegroundColor Yellow
        }
        'service-stopped' {
            Write-Fail 'The service stopped by itself.'
        }
        default {
            Write-Fail 'The service started but did not report healthy in time.'
            Write-Host "  Check $healthUrl in a browser on this server." -ForegroundColor Yellow
        }
    }

    # The reason is in the event log within seconds of the attempt. Printing it here is the
    # difference between "it did not come up" and knowing exactly what to change.
    $fault = Get-LastApplicationFault
    if ($fault) {
        Write-Host ''
        Write-Host '  Windows recorded this:' -ForegroundColor Yellow
        Write-Host "    $fault" -ForegroundColor Yellow
    } else {
        Write-Host "  Event Viewer -> Windows Logs -> Application (source: $ServiceName / .NET Runtime)" -ForegroundColor Yellow
    }
    exit 1
}
Write-Ok 'Service is up and the database is ready'

# ---------------------------------------------------------------------------
# 8. Remove the first-login password from disk.
# ---------------------------------------------------------------------------
$saved = Get-Content $prodConfig -Raw | ConvertFrom-Json
$saved.Bootstrap.AdminPassword = ''
$saved | ConvertTo-Json -Depth 20 | Set-Content $prodConfig -Encoding UTF8

# Stop and start explicitly rather than Restart-Service: restart does not always wait long enough
# for the stop to finish, and then fails with "cannot start service" on a service that is merely
# still shutting down.
Stop-FinoService -ServiceName $ServiceName | Out-Null
$started = Start-FinoService -ServiceName $ServiceName

if (-not $started) {
    Write-Warn2 "The service did not restart after the configuration was updated. Start it from services.msc ($ServiceName)."
} else {
    Write-Ok 'First-login password removed from the configuration file'
}

# ---------------------------------------------------------------------------
# 9. Record what was installed.
#
# This file is how a later run of Setup.bat knows there is already an installation here, which
# version it is, and where its service and database are - so the person upgrading does not have to
# know or retype any of it.
# ---------------------------------------------------------------------------
$installedVersion = Get-ReleaseVersion $configDir
Write-InstallState -InstallPath $InstallPath -State @{
    version        = $installedVersion
    brandName      = $BrandName
    hosting        = 'service'
    serviceName    = $ServiceName
    serviceAccount = $ServiceAccount
    port           = $Port
    scheme         = $scheme
    publicUrl      = $PublicUrl
    sqlServer      = $SqlServer
    databaseName   = $DatabaseName
    installedOn    = (Get-Date).ToString('o')
    installedBy    = "$env:USERNAME@$env:COMPUTERNAME"
}
Write-Ok "Installation recorded (version $installedVersion)"

# ---------------------------------------------------------------------------
# 10. Summary
# ---------------------------------------------------------------------------
$panelAddress = if ($PublicUrl) { $PublicUrl.TrimEnd('/') } else { "${scheme}://$($env:COMPUTERNAME):$Port" }

$summary = @"
$BrandName - installation summary
Version $installedVersion, installed on $(Get-Date -Format 'yyyy-MM-dd HH:mm') by $env:USERNAME

  Panel address : $panelAddress
  Local address : ${scheme}://localhost:$Port
  Health check  : $healthUrl

  First login   : $(if ($SkipAdminSeed) {
                        'the logins that already existed in this database are unchanged'
                    } else {
                        "mobile $AdminMobile
                  password: the one typed during installation
                  (the panel asks for a new password on the first login)"
                    })

  Install folder: $InstallPath
  Service name  : $ServiceName    (services.msc -> $BrandName API)
  Database      : $DatabaseName on $SqlServer

  Daily backup  : run Backup-Fino.ps1 from Task Scheduler
  Change logo   : Set-Branding.ps1
  Upgrades      : run Setup.bat from the new release (backs up first, rolls back if needed)

Keep this file. The keys below belong to this installation only - they are not
shared with any other client, and losing them means every saved session and the
panel's encrypted transport stop matching the server.
"@

$summaryPath = Join-Path $InstallPath 'INSTALLATION-SUMMARY.txt'
$summary | Set-Content $summaryPath -Encoding UTF8

Write-Host ''
Write-Host '  ============================================================' -ForegroundColor Green
Write-Host '    Installation complete' -ForegroundColor Green
Write-Host '  ============================================================' -ForegroundColor Green
Write-Host $summary
Write-Host "  (also saved to $summaryPath)" -ForegroundColor DarkGray
Write-Host ''
Write-Warn2 'Open the panel now and change the administrator password.'
if (-not $CertificatePath) {
    Write-Warn2 'This install serves plain HTTP. Put it behind HTTPS (a certificate, or Cloudflare) before real money moves through it.'
}
