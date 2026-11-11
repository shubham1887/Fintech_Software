<#
.SYNOPSIS
    The one thing to run on a client's server. Installs Fino, or upgrades what is already there.

.DESCRIPTION
    Started by Setup.bat. It works out for itself which of these it is looking at:

      * a clean server            -> installs everything, SQL Server included
      * an installation from this kit -> offers the upgrade, keeping data, keys and branding
      * an older hand-made IIS deployment -> recognises it and explains the move

    The manual sequence this replaces - install IIS, install Node, install the .NET hosting bundle,
    create sites and folders, copy files, edit appsettings, edit the UI config, load the database
    tables, start the site - is gone. IIS, Node and the .NET runtime are not needed at all: the
    release carries its own runtime and hosts itself as a Windows service, the database builds
    itself on first start, and both config files are written from the answers given here.

    What it asks for is only what is genuinely per-client: company name, logo, admin login,
    address and port.

.EXAMPLE
    # What the customer does
    Setup.bat   (right-click, Run as administrator)

.EXAMPLE
    # Your own repeat install, no questions
    .\Setup.ps1 -Unattended -BrandName "RiPay" -AdminMobile "9876543210" `
        -AdminPassword "Str0ng#Pass" -PublicUrl "https://panel.ripay.co.in" `
        -LogoPath "D:\clients\ripay\logo.png"
#>
[CmdletBinding()]
param(
    [string] $InstallPath = 'C:\Fino',
    [string] $BrandName,
    [string] $LogoPath,
    [string] $AdminName = 'Administrator',
    [string] $AdminMobile,
    [string] $AdminEmail,
    [string] $AdminPassword,
    [string] $PublicUrl,
    [int]    $Port = 8080,
    [string] $SqlServer,
    [string] $DatabaseName = 'FinoApp',
    [string] $ServiceName = 'FinoAPI',
    [string] $CertificatePath,
    [string] $CertificatePassword,
    [string] $BackupPath,
    # Left out, the right one is worked out from the server. "menu" and "migrate" are reached that
    # way rather than typed, but they are listed because the validation attribute on this parameter
    # also applies to every later assignment to $Mode.
    [ValidateSet('', 'install', 'update', 'branding', 'menu', 'migrate')]
    [string] $Mode = '',
    # Answers as JSON, instead of on the command line. This is how FinoSetup.exe passes what it
    # collected: a password on a command line is visible to anyone who can list processes, and
    # quoting a company name with an apostrophe in it through two shells is a bug waiting to happen.
    # The file is written by the caller with a restrictive ACL and deleted afterwards.
    [string] $AnswerFile,
    # Print what this server looks like as JSON and exit, changing nothing. The graphical setup
    # calls this to decide which buttons to offer.
    [switch] $Probe,
    # "The person running this has already been shown the warnings and agreed." Set by the graphical
    # setup, which asks in its own dialog. Without it a migration refuses to run unattended, because
    # switching a live deployment over is not something to do without a human saying yes.
    [switch] $ConfirmMigration,
    [switch] $Unattended
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

. (Join-Path $scriptRoot 'lib\FinoCommon.ps1')
. (Join-Path $scriptRoot 'lib\Prerequisites.ps1')
. (Join-Path $scriptRoot 'lib\Branding.ps1')

$appSource      = Join-Path $scriptRoot 'app'
$hasApplication = Test-Path (Join-Path $appSource 'FintechApp.exe')
$releaseVersion = Get-ReleaseVersion $appSource

# Answers from the graphical setup. Only parameters the caller actually set are taken, so a blank
# field in the form does not wipe out a default here.
if ($AnswerFile) {
    if (-not (Test-Path $AnswerFile)) { Write-Fail "Answer file not found: $AnswerFile"; exit 1 }
    $answers = Get-Content $AnswerFile -Raw | ConvertFrom-Json
    foreach ($property in $answers.PSObject.Properties) {
        if ($null -eq $property.Value -or "$($property.Value)" -eq '') { continue }
        Set-Variable -Name $property.Name -Value $property.Value -Scope Script -ErrorAction SilentlyContinue
    }
    $Unattended = $true
}

# ---------------------------------------------------------------------------
# Probe: describe the server as JSON and stop. Used by FinoSetup.exe.
# ---------------------------------------------------------------------------
if ($Probe) {
    $facts    = Get-ServerFacts
    $found    = Find-FinoInstallation -PreferredPath $InstallPath -ServiceName $ServiceName
    $installed = $null
    if ($found) {
        $installed = [ordered]@{
            mode        = $found.mode
            installPath = $found.installPath
            serviceName = $found.serviceName
            siteName    = $found.siteName
            version     = $found.version
            brandName   = if ($found.state) { $found.state.brandName } else { $null }
            port        = if ($found.state) { $found.state.port } else { $null }
            scheme      = if ($found.state) { $found.state.scheme } else { $null }
            publicUrl   = if ($found.state) { $found.state.publicUrl } else { $null }
            database    = if ($found.state) { $found.state.databaseName } else { $null }
            sqlServer   = if ($found.state) { $found.state.sqlServer } else { $null }
            # A stopped or disabled service usually means this box has moved to another hosting
            # (IIS sites, say) and this installation is dormant. Updating it would start a second
            # application against the same database.
            serviceState = $found.serviceState
            serviceStart = $found.serviceStart
        }
    }

    [ordered]@{
        releaseVersion = $releaseVersion
        hasApplication = $hasApplication
        isAdmin        = Test-IsAdministrator
        os             = $facts.OsName
        ramGb          = $facts.RamGb
        freeDiskGb     = $facts.FreeDiskGb
        is64Bit        = $facts.Is64Bit
        sqlInstances   = @($facts.SqlInstances | ForEach-Object {
                            [ordered]@{ name = $_.Name; serverName = $_.ServerName; status = $_.Status } })
        installed      = $installed
        versionCompare = if ($installed -and $installed.version) { Compare-FinoVersion $releaseVersion $installed.version } else { $null }
    } | ConvertTo-Json -Depth 6
    exit 0
}

# Two very different people can end up running this file, and they need different answers when the
# application folder is missing:
#   * the developer, standing in the repository's own deploy folder - no release has been built yet
#   * someone at a client's server whose unzip went wrong
# They are told apart by whether the source tree is sitting next to this script.
$isSourceFolder = (Test-Path (Join-Path $scriptRoot 'Build-Release.ps1')) -and
                  (Test-Path (Join-Path (Split-Path -Parent $scriptRoot) 'Fino_API'))

Write-Banner 'Fino' $(if ($hasApplication) {
        "setup and upgrade  -  release $releaseVersion"
    } elseif ($isSourceFolder) {
        'source folder  -  no release built yet'
    } else {
        'setup and upgrade'
    })

# ---------------------------------------------------------------------------
# 0. Can this even run?
# ---------------------------------------------------------------------------
if (-not $hasApplication -and $isSourceFolder) {
    Write-Host ''
    Write-Host '  This is the source folder, not a release.' -ForegroundColor White
    Write-Host '  Setup installs a BUILT release: the published application has to exist in .\app first.' -ForegroundColor DarkGray
    Write-Host ''
    Write-Host '  Build one:' -ForegroundColor White
    Write-Host '    .\Build-Release.ps1 -Version 2026.09.16.1' -ForegroundColor DarkGray
    Write-Host ''
    Write-Host '  That writes deploy\dist\Fino-<version>-win-x64.zip. Send the client that zip;' -ForegroundColor DarkGray
    Write-Host '  they unzip it and run Setup.bat from inside the unzipped folder.' -ForegroundColor DarkGray
    Write-Host ''

    if (-not $Unattended -and (Ask-YesNo '  Build a release now?' $true)) {
        $suggested = Get-Date -Format 'yyyy.MM.dd.HHmm'
        $version = Ask '  Version' -Default $suggested -Required
        & (Join-Path $scriptRoot 'Build-Release.ps1') -Version $version
        exit $LASTEXITCODE
    }
    exit 1
}

if (-not (Test-IsAdministrator)) {
    Write-Fail 'This window is not running as Administrator.'
    Write-Host '        Close it, right-click Setup.bat and choose "Run as administrator".' -ForegroundColor Yellow
    exit 1
}

if (-not $hasApplication) {
    Write-Fail "The application folder is missing: $appSource"
    Write-Host '        Unzip the whole release first, then run Setup.bat from inside the unzipped folder.' -ForegroundColor Yellow
    Write-Host '        (Running it from inside the .zip does not work - Windows only unpacks one file.)' -ForegroundColor Yellow
    exit 1
}

# ---------------------------------------------------------------------------
# 1. What is on this server already?
# ---------------------------------------------------------------------------
$existing = Find-FinoInstallation -PreferredPath $InstallPath -ServiceName $ServiceName

if (-not $Mode) {
    if (-not $existing) {
        $Mode = 'install'
    } elseif ($existing.mode -eq 'iis') {
        $Mode = 'migrate'
    } else {
        $Mode = 'menu'
    }
}

# ---------------------------------------------------------------------------
# 2. Existing installation: what would you like to do?
# ---------------------------------------------------------------------------
if ($Mode -eq 'menu') {
    $installedVersion = if ($existing.version) { $existing.version } else { 'unknown' }
    $comparison = Compare-FinoVersion $releaseVersion $installedVersion

    Write-Step 'This server already has Fino'
    Write-Note "installed at : $($existing.installPath)"
    Write-Note "version      : $installedVersion"
    Write-Note "this release : $releaseVersion"

    if ($comparison -gt 0) {
        Write-Ok 'This release is newer - an upgrade is available.'
    } elseif ($comparison -eq 0) {
        Write-Note 'The same version is already installed.'
    } else {
        Write-Warn2 'This release is OLDER than what is installed. Going back is not normally what you want.'
    }

    Write-Host ''
    Write-Host '  What would you like to do?' -ForegroundColor White
    Write-Host '    1. Update to this release   (backs up first, puts the old one back if it fails)'
    Write-Host '    2. Change the logo or company name'
    Write-Host '    3. Take a backup now'
    Write-Host '    4. Nothing - close'
    Write-Host ''

    switch (([string](Read-Answer '  Choose 1-4')).Trim()) {
        '1' { $Mode = 'update' }
        '2' { $Mode = 'branding' }
        '3' {
            & (Join-Path $scriptRoot 'Backup-Fino.ps1') -InstallPath $existing.installPath
            exit $LASTEXITCODE
        }
        default { Write-Host '  Nothing changed.' -ForegroundColor DarkGray; exit 0 }
    }
}

# ---------------------------------------------------------------------------
# 2b. An older IIS deployment: move it onto the new hosting, keeping the data.
#
# This is the case for every client installed before this kit existed. The database is not
# touched - the new service is pointed at the same one - and the JWT and AES keys are carried
# across so that nobody is logged out and the panel keeps talking to the API.
#
# The old IIS sites are only ever stopped after an explicit yes, and never in unattended mode:
# on a live server those bindings are the client's business.
# ---------------------------------------------------------------------------
if ($Mode -eq 'migrate') {
    Write-Step 'An existing IIS deployment was found'
    Write-Note "site   : $($existing.siteName)"
    Write-Note "folder : $($existing.installPath)"

    $legacy = Get-LegacyDeploymentSettings $existing.installPath
    if (-not $legacy) {
        Write-Fail 'Its appsettings.json could not be read, so nothing can be carried over safely.'
        Write-Host '  Install fresh instead, and give it the same SQL Server and database name:' -ForegroundColor Yellow
        Write-Host '    Setup.bat   (then answer the questions)' -ForegroundColor DarkGray
        exit 1
    }

    Write-Ok "Database   : $($legacy.DatabaseName) on $($legacy.SqlServer)"
    Write-Ok ("Keys found : {0}" -f $(if ($legacy.JwtSecret -and $legacy.AesKey) { 'yes - operators stay signed in' } else { 'no - everyone signs in again after the move' }))

    Write-Host ''
    Write-Host '  What the move does:' -ForegroundColor White
    Write-Host '    - installs this release as a Windows service (no IIS, no .NET install needed)' -ForegroundColor DarkGray
    Write-Host '    - points it at the SAME database, so no data moves and nothing is lost' -ForegroundColor DarkGray
    Write-Host '    - applies any new database changes on first start, by itself' -ForegroundColor DarkGray
    Write-Host '    - serves the panel and the API together on one address' -ForegroundColor DarkGray
    Write-Host ''
    Write-Warn2 'Do this in a maintenance window: the platform is down for a few minutes.'
    Write-Warn2 'Take a database backup first if there is not a recent one.'
    Write-Host ''

    if ($Unattended -and -not $ConfirmMigration) {
        Write-Fail 'A migration is not done unattended - it needs someone to confirm the switchover.'
        Write-Host '  Run Setup.bat (or FinoSetup.exe) and confirm there.' -ForegroundColor Yellow
        exit 1
    }

    if (-not $ConfirmMigration) {
        if (-not (Ask-YesNo '  Move this deployment to the new setup now?' $false)) {
            Write-Host '  Nothing changed.' -ForegroundColor DarkGray
            exit 0
        }

        if (-not $BrandName) { $BrandName = Ask "`n  Company / brand name shown in the panel" -Required }
        if (-not $LogoPath)  { $LogoPath  = Read-LogoPath -AllowSkip }

        Write-Host ''
        Write-Host '  The old site is on port 80/443. The new service needs its own port.' -ForegroundColor DarkGray
        Write-Host '  Keep the old sites stopped and use 80, or run side by side on 8080 first and switch later.' -ForegroundColor DarkGray
        $Port = [int](Ask '  Port for the new service' -Default "$Port" -Required)
    }

    if (-not $BrandName) { Write-Fail 'A company name is needed for the panel.'; exit 1 }

    # A backup before anything touches that database. The new build applies its own schema changes
    # on first start, and "we upgraded and it broke" without a backup is not a position to be in
    # with someone's live ledger.
    Write-Step 'Backing up the database first'
    $backupDir = Join-Path 'C:\FinoBackups' ('pre-migration-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    try {
        $backup = Invoke-DatabaseBackup -ConnectionString $legacy.ConnectionString `
                                        -TargetPath (Join-Path $backupDir "$($legacy.DatabaseName)-pre-migration.bak") `
                                        -Label 'Fino pre-migration backup'
        Write-Ok "Backed up to $($backup.Path)"
    } catch {
        Write-Fail "The database could not be backed up: $($_.Exception.Message)"
        Write-Host '  The migration has been stopped. Nothing was changed.' -ForegroundColor Yellow
        exit 1
    }

    # The people who use this platform are already in that database, so nothing is seeded and no
    # existing login is touched.
    $migrateArgs = @{
        Unattended    = $true
        SkipAdminSeed = $true
        BrandName     = $BrandName
        InstallPath   = $InstallPath
        Port          = $Port
        SqlServer     = $legacy.SqlServer
        DatabaseName  = $legacy.DatabaseName
        ServiceName   = $ServiceName
    }
    if ($LogoPath)          { $migrateArgs['LogoPath']  = $LogoPath }
    if ($PublicUrl)         { $migrateArgs['PublicUrl'] = $PublicUrl }
    if ($legacy.JwtSecret)  { $migrateArgs['JwtSecret'] = $legacy.JwtSecret }
    if ($legacy.AesKey)     { $migrateArgs['AesKey']    = $legacy.AesKey }
    if ($legacy.AesIv)      { $migrateArgs['AesIv']     = $legacy.AesIv }

    & (Join-Path $scriptRoot 'Install-Fino.ps1') @migrateArgs
    $migrateExit = $LASTEXITCODE
    if ($migrateExit -ne 0) {
        Write-Fail 'The migration did not finish. The old IIS site has NOT been touched, so the platform is still up.'
        exit $migrateExit
    }

    Write-Step 'The old IIS sites'
    Write-Host '  The new service is running. The old sites are still serving the same database,' -ForegroundColor DarkGray
    Write-Host '  which is fine for a few minutes but should not stay that way.' -ForegroundColor DarkGray
    Write-Host ''
    Write-Host "  API site: $($existing.siteName)" -ForegroundColor White
    if (Ask-YesNo "  Stop it now?" $false) {
        & "$env:SystemRoot\System32\inetsrv\appcmd.exe" stop site "$($existing.siteName)" | Out-Null
        Write-Ok "Stopped $($existing.siteName)"
        Write-Note ('To put it back:  appcmd start site "{0}"' -f $existing.siteName)
    } else {
        Write-Note 'Left running - stop it from IIS Manager when you are ready.'
    }

    Write-Host ''
    Write-Host '  The old UI site (the panel) is now served by the new service as well, so it can be' -ForegroundColor DarkGray
    Write-Host '  stopped too - from IIS Manager, once the new address has been checked.' -ForegroundColor DarkGray
    Write-Host ''
    exit 0
}

# ---------------------------------------------------------------------------
# 3. Branding only
# ---------------------------------------------------------------------------
if ($Mode -eq 'branding') {
    & (Join-Path $scriptRoot 'Set-Branding.ps1') -InstallPath $existing.installPath -BrandName $BrandName -LogoPath $LogoPath
    exit $LASTEXITCODE
}

# ---------------------------------------------------------------------------
# 4. Update
# ---------------------------------------------------------------------------
if ($Mode -eq 'update') {
    if (-not $existing) {
        Write-Fail 'There is nothing installed on this server to update.'
        exit 1
    }

    $installedVersion = if ($existing.version) { $existing.version } else { 'unknown' }
    if ((Compare-FinoVersion $releaseVersion $installedVersion) -lt 0 -and -not $Unattended) {
        Write-Warn2 "You are about to replace version $installedVersion with the older $releaseVersion."
        if (-not (Ask-YesNo '  Are you sure?' $false)) { exit 0 }
    }

    $updateArgs = @{
        InstallPath = $existing.installPath
        ServiceName = if ($existing.serviceName) { $existing.serviceName } else { $ServiceName }
    }
    if ($BackupPath) { $updateArgs['BackupPath'] = $BackupPath }

    & (Join-Path $scriptRoot 'Update-Fino.ps1') @updateArgs
    exit $LASTEXITCODE
}

# ---------------------------------------------------------------------------
# 5. Fresh installation
#
# Two ways to host it, from this one package:
#   1. as a Windows service (recommended) - nothing else to install, this script does all of it
#   2. as two IIS sites - for a client who already has api.* and ui.* with their own certificates
# The second hands over to Configure-Split.ps1, which copies this same build into the two site
# folders and writes both configuration files with matching keys.
# ---------------------------------------------------------------------------
if (-not $Unattended -and (Test-Path (Join-Path $scriptRoot 'Configure-Split.ps1'))) {
    Write-Step 'How should this be hosted?'
    Write-Host '    1. As a Windows service   (recommended - no IIS setup, starts on boot)'
    Write-Host '    2. As two IIS sites       (api.client.com + ui.client.com, you already have them)'
    Write-Host ''
    if ((([string](Read-Answer '  Choose 1-2')).Trim()) -eq '2') {
        $splitArgs = @{}
        if ($BrandName) { $splitArgs['BrandName'] = $BrandName }
        if ($LogoPath)  { $splitArgs['LogoPath']  = $LogoPath }
        & (Join-Path $scriptRoot 'Configure-Split.ps1') @splitArgs
        exit $LASTEXITCODE
    }
}

$facts = Get-ServerFacts
if (-not (Show-ServerCheck $facts)) {
    Write-Fail 'This server does not meet the minimum requirements.'
    exit 1
}

# --- 5a. the database engine --------------------------------------------------
Write-Step 'Database engine'
$resolvedSqlServer = Resolve-SqlServerForInstall -Facts $facts -Unattended:$Unattended -PreferredServer $SqlServer
if (-not $resolvedSqlServer) {
    Write-Fail 'Setup stopped: no database engine.'
    exit 1
}

# --- 5b. the questions --------------------------------------------------------
if (-not $Unattended) {
    Write-Step 'Client details'
    Write-Host '  (press Enter to accept the value in brackets)' -ForegroundColor DarkGray

    if (-not $BrandName) { $BrandName = Ask "`n  Company / brand name shown in the panel" -Required }
    if (-not $LogoPath)  { $LogoPath  = Read-LogoPath -AllowSkip }

    Write-Host ''
    Write-Host '  First administrator login (the client''s owner account):' -ForegroundColor White
    if (-not $AdminMobile) { $AdminMobile = Ask '  Mobile number' -Required }
    if (-not $AdminEmail)  { $AdminEmail  = Ask '  Email address' -Default "admin@$(($BrandName -replace '[^A-Za-z0-9]','').ToLower()).local" }
    if (-not $AdminPassword) {
        while ($true) {
            $AdminPassword = Ask '  Password (at least 8 characters)' -Required -Secret
            $again         = Ask '  Password again' -Required -Secret
            if ($AdminPassword -ne $again)   { Write-Warn2 'The two passwords do not match.'; continue }
            if ($AdminPassword.Length -lt 8) { Write-Warn2 'Too short - use at least 8 characters.'; continue }
            break
        }
    }

    Write-Step 'Where it will run'
    if (-not $PublicUrl) {
        Write-Host '  The web address the client will type, for example https://panel.theircompany.com' -ForegroundColor DarkGray
        Write-Host '  Leave it blank if they will reach it by IP address on the local network.' -ForegroundColor DarkGray
        $PublicUrl = Ask '  Panel address' -Default $PublicUrl
    }
    Write-Host '  The port the panel is reached on - NOT the database port.' -ForegroundColor DarkGray
    $Port         = [int](Ask '  Port' -Default "$Port" -Required)
    $DatabaseName = Ask '  Database name (created for you)' -Default $DatabaseName -Required
    $InstallPath  = Ask '  Install folder' -Default $InstallPath -Required
}

foreach ($required in @(
    @{ name = 'Company name';  value = $BrandName },
    @{ name = 'Admin mobile';  value = $AdminMobile },
    @{ name = 'Admin password'; value = $AdminPassword })) {
    if (-not $required.value) { Write-Fail "$($required.name) is required."; exit 1 }
}

# --- 5c. hand over to the installer ------------------------------------------
$installArgs = @{
    Unattended    = $true          # the questions have been asked here
    BrandName     = $BrandName
    InstallPath   = $InstallPath
    Port          = $Port
    SqlServer     = $resolvedSqlServer
    DatabaseName  = $DatabaseName
    AdminName     = $AdminName
    AdminMobile   = $AdminMobile
    AdminEmail    = $AdminEmail
    AdminPassword = $AdminPassword
    ServiceName   = $ServiceName
}
if ($PublicUrl)           { $installArgs['PublicUrl'] = $PublicUrl }
if ($LogoPath)            { $installArgs['LogoPath'] = $LogoPath }
if ($CertificatePath)     { $installArgs['CertificatePath'] = $CertificatePath }
if ($CertificatePassword) { $installArgs['CertificatePassword'] = $CertificatePassword }

& (Join-Path $scriptRoot 'Install-Fino.ps1') @installArgs
$installExit = $LASTEXITCODE

if ($installExit -ne 0) {
    Write-Host ''
    Write-Fail 'The installation did not finish.'
    Write-Host '  Nothing else has to be undone by hand - fix what the message above says and run Setup.bat again.' -ForegroundColor Yellow
    exit $installExit
}

# --- 5d. the things a live payments box should not be without -----------------
Write-Step 'Finishing touches'

$backupScript = Join-Path $InstallPath 'Backup-Fino.ps1'
foreach ($helper in @('Backup-Fino.ps1', 'Update-Fino.ps1', 'Set-Branding.ps1', 'Setup.ps1', 'Setup.bat')) {
    $source = Join-Path $scriptRoot $helper
    if (Test-Path $source) { Copy-Item $source (Join-Path $InstallPath $helper) -Force }
}
if (Test-Path (Join-Path $scriptRoot 'lib')) {
    Copy-Item (Join-Path $scriptRoot 'lib') (Join-Path $InstallPath 'lib') -Recurse -Force
}
Write-Ok 'Maintenance scripts copied into the install folder'

# A nightly backup, scheduled now rather than "later". Later does not happen, and the first thing
# anyone asks after a disk failure is when the last backup ran.
$taskName = "$BrandName nightly backup"
try {
    if (Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue) {
        Write-Ok 'Nightly backup already scheduled'
    } else {
        # Register-ScheduledTask rather than schtasks.exe: the command line here contains quoted
        # paths, and passing those through schtasks /tr mangles the quoting often enough that the
        # task registers and then fails silently at 2am - the worst possible time to find out.
        $action = New-ScheduledTaskAction -Execute 'powershell.exe' `
            -Argument ("-NoProfile -ExecutionPolicy Bypass -File `"{0}`" -InstallPath `"{1}`"" -f $backupScript, $InstallPath)
        $trigger   = New-ScheduledTaskTrigger -Daily -At 2am
        $principal = New-ScheduledTaskPrincipal -UserId 'SYSTEM' -LogonType ServiceAccount -RunLevel Highest
        $settings  = New-ScheduledTaskSettingsSet -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Hours 3)

        Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger `
            -Principal $principal -Settings $settings `
            -Description "$BrandName - nightly database, uploads and key backup" | Out-Null

        Write-Ok 'Nightly backup scheduled for 02:00 (Task Scheduler)'
    }
} catch {
    Write-Warn2 "Could not schedule the nightly backup: $($_.Exception.Message)"
    Write-Warn2 'Set it up by hand from Task Scheduler, pointing at Backup-Fino.ps1.'
}

Write-Host ''
Write-Host '  Backups are written to C:\FinoBackups. Copy them off this server regularly -' -ForegroundColor Yellow
Write-Host '  a backup that only exists on this machine does not survive this machine.' -ForegroundColor Yellow
Write-Host ''
exit 0
