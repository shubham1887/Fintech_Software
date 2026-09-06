<#
.SYNOPSIS
    Adds a new white-label client (so its own APK can be built).

.EXAMPLE
    .\tools\new-client.ps1 -Id sharma -Name "Fino Console" -ApplicationId com.FinoConsole.app `
        -Url https://sharmapay.in -Primary "#7C3AED" -Accent "#A78BFA" -WhatsApp 919812345678

    Then:
        .\gradlew assembleSharmaRelease
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Id,
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][string]$ApplicationId,
    [Parameter(Mandatory = $true)][string]$Url,
    [string]$Primary = "#1D4ED8",
    [string]$Accent = "#0EA5E9",
    [string]$Splash = "#0B1220",
    # Note: Id must be lowercase - it becomes the Gradle flavor name
    [string]$WhatsApp = "",
    [string]$Email = "",
    [string]$Phone = "",
    [int]$VersionCode = 1,
    [string]$VersionName = "1.0.0"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$registry = Join-Path $root "whitelabel\clients.json"

if (-not (Test-Path $registry)) { throw "clients.json not found: $registry" }

# The flavor name has to be a valid Java identifier
if ($Id -notmatch '^[a-z][a-z0-9]*$') {
    throw "Id may only contain lowercase letters and digits (e.g. sharma, paykart1). Got: $Id"
}

$json = Get-Content $registry -Raw -Encoding UTF8 | ConvertFrom-Json

$existing = $json.clients | Where-Object { $_.id -eq $Id }

$clash = $json.clients | Where-Object { $_.applicationId -eq $ApplicationId -and $_.id -ne $Id }
if ($clash) {
    throw "applicationId '$ApplicationId' is already used by '$($clash.id)'."
}

$client = [ordered]@{
    id              = $Id
    appName         = $Name
    applicationId   = $ApplicationId
    portalUrl       = $Url.TrimEnd('/')
    colorPrimary    = $Primary
    colorAccent     = $Accent
    colorSplash     = $Splash
    supportWhatsApp = $WhatsApp
    supportEmail    = $Email
    supportPhone    = $Phone
    versionCode     = $VersionCode
    versionName     = $VersionName
}

if ($existing) {
    # Already registered -> update it in place instead of adding a duplicate
    $others = @($json.clients | Where-Object { $_.id -ne $Id })
    $json.clients = @($others + [pscustomobject]$client)
    $action = "updated"
} else {
    $json.clients += [pscustomobject]$client
    $action = "added"
}
$json | ConvertTo-Json -Depth 6 | Out-File $registry -Encoding utf8

# Per-client asset folder (drop a logo / icon override in here)
$resDir = Join-Path $root "app\src\$Id\res\drawable"
New-Item -ItemType Directory -Force -Path $resDir | Out-Null

$flavor = $Id.Substring(0,1).ToUpper() + $Id.Substring(1)

Write-Host ""
Write-Host "Client '$Name' $action." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. To change the logo, drop these files in (keep the same names):"
Write-Host "     $resDir\ic_brand_mark.xml"
Write-Host "     $resDir\ic_launcher_foreground.xml"
Write-Host "  2. Host app-config.json on the client server:"
Write-Host "     $($Url.TrimEnd('/'))/app-config.json   (sample: whitelabel\app-config.sample.json)"
Write-Host "  3. Build the APK:"
Write-Host "     .\gradlew assemble$($flavor)Release"
Write-Host ""
