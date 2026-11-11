<#
.SYNOPSIS
    Backs up a Fino installation: the database, the uploaded files and the configuration.

.DESCRIPTION
    Reads the connection string out of the installation's own appsettings.Production.json, so it
    needs no arguments beyond the install path, and cannot drift from what the service actually
    uses.

    Meant to be run from Task Scheduler every night:

        schtasks /create /tn "Fino nightly backup" /sc daily /st 02:00 /ru SYSTEM ^
          /tr "powershell -NoProfile -ExecutionPolicy Bypass -File C:\Fino\Backup-Fino.ps1"

    A backup nobody has restored is a guess, not a backup - restore one into a spare database
    once, and note how long it took.

.EXAMPLE
    .\Backup-Fino.ps1 -InstallPath C:\Fino -BackupRoot D:\Backups\Fino -KeepDays 14
#>
[CmdletBinding()]
param(
    [string] $InstallPath = 'C:\Fino',
    [string] $BackupRoot  = 'C:\FinoBackups',
    [int]    $KeepDays    = 14
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $scriptRoot 'lib\FinoCommon.ps1')

$config = Join-Path $InstallPath 'app\appsettings.Production.json'
if (-not (Test-Path $config)) { Write-Fail "No installation found at $InstallPath."; exit 1 }

$settings         = Get-Content $config -Raw | ConvertFrom-Json
$connectionString = $settings.ConnectionStrings.DefaultConnection
$database         = Get-DatabaseNameFromConnectionString $connectionString
if (-not $database) {
    Write-Fail 'Could not read the database name out of the connection string.'
    exit 1
}

$stamp     = Get-Date -Format 'yyyyMMdd-HHmmss'
$targetDir = Join-Path $BackupRoot $stamp
New-Item -ItemType Directory -Path $targetDir -Force | Out-Null

Write-Host "`nFino backup -> $targetDir" -ForegroundColor Cyan

# ---------------------------------------------------------------------------
# Database
# ---------------------------------------------------------------------------
$bakPath = Join-Path $targetDir "$database-$stamp.bak"

try {
    $result = Invoke-DatabaseBackup -ConnectionString $connectionString -TargetPath $bakPath
    if ($result.OnThisHost) {
        Write-Ok ("database: {0} ({1} MB{2})" -f (Split-Path -Leaf $result.Path),
                    [math]::Round((Get-Item $result.Path).Length / 1MB, 1),
                    $(if ($result.Compressed) { '' } else { ', uncompressed' }))
    } else {
        Write-Warn2 "The backup is on the SQL Server host at $($result.Path) - it could not be copied here."
    }
} catch {
    Write-Fail "database backup failed: $($_.Exception.Message)"
    exit 1
}

# ---------------------------------------------------------------------------
# Uploaded files (KYC documents, icons) and the configuration.
# ---------------------------------------------------------------------------
$uploads = Join-Path $InstallPath 'app\wwwroot\uploads'
if (Test-Path $uploads) {
    $zip = Join-Path $targetDir "uploads-$stamp.zip"
    Compress-Archive -Path (Join-Path $uploads '*') -DestinationPath $zip -CompressionLevel Optimal -ErrorAction SilentlyContinue
    if (Test-Path $zip) { Write-Ok ("uploads: {0} MB" -f [math]::Round((Get-Item $zip).Length / 1MB, 1)) }
} else {
    Write-Host '  [ ]    no uploads folder yet' -ForegroundColor DarkGray
}

# The configuration holds the signing keys and the panel's AES key; without it a restored database
# is unusable, because every existing session and the panel transport stop matching.
Copy-Item $config (Join-Path $targetDir 'appsettings.Production.json') -Force
Write-Ok 'configuration'

# ---------------------------------------------------------------------------
# Retention
# ---------------------------------------------------------------------------
$cutoff = (Get-Date).AddDays(-$KeepDays)
$removed = 0
Get-ChildItem $BackupRoot -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^\d{8}-\d{6}$' -and $_.CreationTime -lt $cutoff } |
    ForEach-Object { Remove-Item $_.FullName -Recurse -Force; $removed++ }
if ($removed) { Write-Host "  [ ]    removed $removed backup(s) older than $KeepDays days" -ForegroundColor DarkGray }

Write-Host "`nBackup complete: $targetDir" -ForegroundColor Green
Write-Host 'Copy this folder off the server - a backup on the same machine does not survive the machine.' -ForegroundColor Yellow

# Explicit success code: Update-Fino.ps1 gates the upgrade on this, and a script that simply
# runs off the end leaves $LASTEXITCODE holding whatever the last native command set.
exit 0
