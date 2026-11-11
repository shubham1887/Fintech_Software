<#
.SYNOPSIS
    Changes the company name and logo of an installed copy of Fino.

.DESCRIPTION
    Run this when a client sends their logo after the install, or changes their name or artwork
    later. It takes PNG, JPG or SVG; it makes the panel logo, the public site logo and the browser
    tab icon from that one file.

    Nothing is rebuilt and nothing is restarted: the panel reads its branding in the browser, so the
    change is live on the next refresh (tell them to press Ctrl+F5 once).

    A .cdr, .ai or .psd cannot be used directly - the script says so and tells you exactly what to
    ask the client's designer for.

.EXAMPLE
    # Interactive - drag the logo file into the window when asked
    .\Set-Branding.ps1

.EXAMPLE
    .\Set-Branding.ps1 -BrandName "RiPay" -LogoPath "D:\clients\ripay\logo.png"
#>
[CmdletBinding()]
param(
    [string] $InstallPath = 'C:\Fino',
    [string] $BrandName,
    [string] $LogoPath,
    # The square artwork the phone app is built around. Optional: without it the app shows the
    # wide logo, same as the website.
    [string] $AppIconPath,
    [string] $Tagline = 'Console',
    [string] $LoginHeadline,
    [string] $LoginSubtext
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $scriptRoot 'lib\FinoCommon.ps1')
. (Join-Path $scriptRoot 'lib\Branding.ps1')

Write-Banner 'Fino - branding'

if (-not (Test-IsAdministrator)) {
    Write-Fail 'This window is not running as Administrator.'
    Write-Host '        Right-click PowerShell, choose "Run as administrator", and run this again.' -ForegroundColor Yellow
    exit 1
}

# Find the installation, rather than trusting the default path.
$found = Find-FinoInstallation -PreferredPath $InstallPath
if ($found -and $found.installPath) { $InstallPath = $found.installPath }

$configPath = Join-Path $InstallPath 'app\wwwroot\panel\config.json'
if (-not (Test-Path $configPath)) {
    Write-Fail "No installation found at $InstallPath."
    Write-Host '  Pass the right folder:  .\Set-Branding.ps1 -InstallPath D:\Fino' -ForegroundColor Yellow
    exit 1
}

$current = Get-Content $configPath -Raw | ConvertFrom-Json
Write-Ok "Installation found at $InstallPath"
Write-Note ("current name: {0}" -f $current.appName)
Write-Note ("current logo: {0}" -f $(if ($current.logoUrl) { $current.logoUrl } else { '(none - name shown as text)' }))

if (-not $BrandName) {
    $BrandName = Ask "`n  Company name shown in the panel" -Default $current.appName -Required
}

if (-not $LogoPath) {
    if (Ask-YesNo "`n  Set or replace the logo?" $true) {
        $LogoPath = Read-LogoPath -AllowSkip
    }
}

Write-Step 'Applying'
$result = Set-FinoBranding -InstallPath $InstallPath -BrandName $BrandName -LogoPath $LogoPath -AppIconPath $AppIconPath `
                           -Tagline $Tagline -LoginHeadline $LoginHeadline -LoginSubtext $LoginSubtext

Write-Ok "Name: $($result.BrandName)"
Write-Ok ("Logo: {0}" -f $(if ($result.LogoUrl) { $result.LogoUrl } else { '(none)' }))

# Keep the recorded state honest - the summary and any later upgrade read the name from here.
$state = Read-InstallState $InstallPath
if ($state) {
    $hash = @{}
    foreach ($property in $state.PSObject.Properties) { $hash[$property.Name] = $property.Value }
    $hash['brandName'] = $BrandName
    Write-InstallState -InstallPath $InstallPath -State $hash
}

Write-Host ''
Write-Host '  Done. Open the panel and press Ctrl+F5 once to see it.' -ForegroundColor Green
Write-Host '  No restart and no reinstall is needed.' -ForegroundColor DarkGray
exit 0
