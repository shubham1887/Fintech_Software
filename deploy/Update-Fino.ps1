<#
.SYNOPSIS
    Upgrades an installed Fino to a newer release, with a backup and an automatic rollback.

.DESCRIPTION
    Run this from inside the NEW release folder (the one containing app\), as Administrator.

    Order of events - the order matters, because the risky step is last and reversible:
      1. back up the database and the current application folder
      2. stop the service
      3. replace the program files, keeping appsettings.Production.json and wwwroot\uploads
      4. start the service - new schema scripts are applied automatically on this start
      5. wait for /health; if it does not come up, put the previous files back and restart

    The database is NOT rolled back automatically. Schema scripts are add-only, so the previous
    build runs against the newer schema; if a release ever needs a destructive change, restore the
    .bak this script wrote.

.EXAMPLE
    .\Update-Fino.ps1 -InstallPath C:\Fino
#>
[CmdletBinding()]
param(
    [string] $InstallPath = 'C:\Fino',
    [string] $ServiceName = 'FinoAPI',
    [string] $BackupPath,
    [switch] $SkipBackup
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $scriptRoot 'lib\FinoCommon.ps1')

if (-not (Test-IsAdministrator)) { Write-Fail 'Run this as Administrator.'; exit 1 }

$appSource = Join-Path $scriptRoot 'app'
$appDir    = Join-Path $InstallPath 'app'
$config    = Join-Path $appDir 'appsettings.Production.json'

if (-not (Test-Path (Join-Path $appSource 'FintechApp.exe'))) { Write-Fail "No new build found at $appSource."; exit 1 }
if (-not (Test-Path $config)) { Write-Fail "No installation found at $InstallPath (expected $config)."; exit 1 }
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $service) { Write-Fail "Service '$ServiceName' is not installed."; exit 1 }

# A disabled service cannot be started, so the update would replace the files, fail to start, roll
# back, fail to start again, and report the platform down - when in fact it was deliberately
# switched off because something else (an IIS site) serves this database now.
if ($service.StartType -eq 'Disabled') {
    Write-Fail "The '$ServiceName' service is DISABLED, so this update cannot start it afterwards."
    Write-Host ''
    Write-Host '  It was probably disabled on purpose because this server now serves the platform' -ForegroundColor Yellow
    Write-Host '  another way - IIS sites, for example. Check which copy should be running before' -ForegroundColor Yellow
    Write-Host '  updating this one: two applications on one database can debit a wallet twice.' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '  If this really is the copy to keep, enable it first:' -ForegroundColor Yellow
    Write-Host "      Set-Service $ServiceName -StartupType Automatic" -ForegroundColor DarkGray
    exit 1
}

$settings = Get-Content $config -Raw | ConvertFrom-Json
$port = 8080
$scheme = 'http'
if ($settings.Kestrel.Endpoints.PSObject.Properties.Name -contains 'Https') {
    $scheme = 'https'; $port = [int](($settings.Kestrel.Endpoints.Https.Url -split ':')[-1])
} elseif ($settings.Kestrel.Endpoints.PSObject.Properties.Name -contains 'Http') {
    $port = [int](($settings.Kestrel.Endpoints.Http.Url -split ':')[-1])
}
$healthUrl = "${scheme}://localhost:$port/health"

$oldVersion = Get-ReleaseVersion $appDir
$newVersion = Get-ReleaseVersion $appSource

Write-Host "`n  Upgrading $InstallPath" -ForegroundColor White
Write-Host "    from version $oldVersion"
Write-Host "    to   version $newVersion"

if ((Compare-FinoVersion $newVersion $oldVersion) -eq 0 -and $oldVersion -ne 'unknown') {
    Write-Note 'Same version - the files will be replaced with identical ones. Harmless, just not useful.'
}

# ---------------------------------------------------------------------------
# 1. Backup
# ---------------------------------------------------------------------------
if (-not $SkipBackup) {
    Write-Step 'Backing up'
    $backupScript = Join-Path $scriptRoot 'Backup-Fino.ps1'
    if (-not (Test-Path $backupScript)) {
        Write-Fail 'Backup-Fino.ps1 is not next to this script, so no backup can be taken.'
        Write-Host '  Re-run with -SkipBackup if you have a backup already.' -ForegroundColor Yellow
        exit 1
    }

    $backupArgs = @{ InstallPath = $InstallPath }
    if ($BackupPath) { $backupArgs['BackupRoot'] = $BackupPath }
    & $backupScript @backupArgs

    # The safety story of this script is "there is a backup first" - so a failed backup stops the
    # upgrade rather than being a line of red text somebody scrolls past.
    if ($LASTEXITCODE -ne 0) {
        Write-Fail 'The backup failed, so the upgrade has been stopped. Nothing has been changed.'
        Write-Host '  Fix the backup and run this again, or re-run with -SkipBackup to upgrade anyway.' -ForegroundColor Yellow
        exit 1
    }
}

$rollbackDir = Join-Path $env:TEMP "fino-rollback-$(Get-Date -Format 'yyyyMMddHHmmss')"
Write-Step 'Keeping a copy of the current build for rollback'
Copy-Item $appDir $rollbackDir -Recurse -Force
Write-Ok $rollbackDir

# ---------------------------------------------------------------------------
# 2-3. Stop and swap
# ---------------------------------------------------------------------------
Write-Step 'Stopping the service'
if (-not (Stop-FinoService -ServiceName $ServiceName)) {
    Write-Fail 'The service would not stop. Nothing has been changed.'
    exit 1
}
Write-Ok 'Stopped'

Write-Step 'Replacing the program files'
# These belong to the installation, not to the release: the configuration, everything operators
# uploaded (KYC documents, icons) and the client's branding - all of which a release must not
# overwrite, or the client comes back rebranded as "Fino" with no logo.
$preserve = @('appsettings.Production.json', 'wwwroot\uploads', 'wwwroot\panel\config.json',
              'wwwroot\panel\favicon.svg', 'wwwroot\panel\favicon.ico')
$keep = Join-Path $env:TEMP "fino-keep-$(Get-Random)"
foreach ($rel in $preserve) {
    $src = Join-Path $appDir $rel
    if (Test-Path $src) {
        $dst = Join-Path $keep $rel
        New-Item -ItemType Directory -Path (Split-Path -Parent $dst) -Force | Out-Null
        Copy-Item $src $dst -Recurse -Force
    }
}

Copy-Item (Join-Path $appSource '*') $appDir -Recurse -Force

foreach ($rel in $preserve) {
    $src = Join-Path $keep $rel
    if (Test-Path $src) { Copy-Item $src (Join-Path $appDir $rel) -Recurse -Force }
}
Remove-Item $keep -Recurse -Force -ErrorAction SilentlyContinue
Write-Ok 'Files replaced, configuration and uploads kept'

# ---------------------------------------------------------------------------
# 4-5. Start and verify
# ---------------------------------------------------------------------------
Write-Step 'Starting the new build'
Write-Host '  A release that adds database changes applies them now; give it a few minutes.' -ForegroundColor DarkGray
Start-FinoService -ServiceName $ServiceName | Out-Null

$health = Wait-FinoHealthy -HealthUrl $healthUrl -ServiceName $ServiceName -TimeoutMinutes 10

if ($health.ok) {
    Write-Ok "Version $newVersion is live"

    # Record the new version, so the next run of Setup.bat compares against the truth.
    $state = Read-InstallState $InstallPath
    $hash = @{}
    if ($state) { foreach ($property in $state.PSObject.Properties) { $hash[$property.Name] = $property.Value } }
    $hash['version']         = $newVersion
    $hash['previousVersion'] = $oldVersion
    $hash['updatedBy']       = "$env:USERNAME@$env:COMPUTERNAME"
    if (-not $hash.ContainsKey('hosting'))     { $hash['hosting'] = 'service' }
    if (-not $hash.ContainsKey('serviceName')) { $hash['serviceName'] = $ServiceName }
    Write-InstallState -InstallPath $InstallPath -State $hash

    Write-Host "`n  Rollback copy kept at $rollbackDir - delete it once you are happy." -ForegroundColor DarkGray
    exit 0
}

if ($health.reason -eq 'setup-failed') {
    Write-Fail "The new build could not apply its database changes: $($health.error)"
}

# Why it did not come up, from the event log, before the rollback buries the evidence. A
# configuration problem - a port that is taken, a database that moved - fails the new build AND the
# old one, because the configuration is shared and an upgrade deliberately keeps it.
$fault = Get-LastApplicationFault
if ($fault) {
    Write-Host ''
    Write-Host '  Windows recorded this:' -ForegroundColor Yellow
    Write-Host "    $fault" -ForegroundColor Yellow
    Write-Host ''
}

Write-Fail 'The new build did not come up. Rolling back.'
Stop-FinoService -ServiceName $ServiceName | Out-Null
Remove-Item (Join-Path $appDir '*') -Recurse -Force
Copy-Item (Join-Path $rollbackDir '*') $appDir -Recurse -Force
Start-FinoService -ServiceName $ServiceName | Out-Null

$restore = Wait-FinoHealthy -HealthUrl $healthUrl -ServiceName $ServiceName -TimeoutMinutes 5

if ($restore.ok) {
    Write-Host "`n  Rolled back to version $oldVersion - the platform is running again." -ForegroundColor Yellow
    Write-Host '  Send the Application event log entries from the failed start to support.' -ForegroundColor Yellow
    exit 1
}

Write-Fail 'The rollback also failed to come up. The platform is DOWN.'
Write-Host ''
Write-Host '  Both builds failing the same way means the problem is in the configuration or the' -ForegroundColor Yellow
Write-Host '  environment, not in the new release - an upgrade keeps the configuration, so a port' -ForegroundColor Yellow
Write-Host '  that is taken or a database that moved breaks the old build exactly as it breaks the new.' -ForegroundColor Yellow
Write-Host ''
Write-Host "  Configuration : $config" -ForegroundColor Yellow
Write-Host "  Previous build: $rollbackDir" -ForegroundColor Yellow
if ($fault) {
    Write-Host ''
    Write-Host '  Windows recorded this:' -ForegroundColor Yellow
    Write-Host "    $fault" -ForegroundColor Yellow
}
exit 2
