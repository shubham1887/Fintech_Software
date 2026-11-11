<#
    Second deploy of the 10 Nov 2026 issue fixes: GST document cancel (number reuse), Commission
    Slabs clean-up, screenshot unblocking and the new Android build.
    Run from an elevated PowerShell:
      powershell -ExecutionPolicy Bypass -File .\deploy\Deploy-IssueFixes-2026-11-10b.ps1

    Steps:
      1. UI  : Fino_Web\dist -> C:\inetpub\wwwroot\ui.finocore.net (keeps config.json / app-config.json / web.config / favicon.ico)
      2. App config: sets "blockScreenshots": false in the LIVE app-config.json (the app reads it on
         every launch, so screenshots work on the installed app without a reinstall)
      3. API : stop pool -> copy deploy\publish-api-finocore -> start pool -> /health -> DLL hash check
      4. APK : the fresh finocore build (versionCode 3 / 1.0.2) -> ui.finocore.net\app\finocore.apk and
         deploy\prebuilt-apk\finocore.apk, after checking it is signed with the SAME key as the APK already
         on the site (a different key would force every user to uninstall first).
#>
$ErrorActionPreference = 'Stop'
# Everything this window prints is also written here, so the result can be checked afterwards.
New-Item -ItemType Directory -Force 'C:\Backups' | Out-Null
Start-Transcript -Path 'C:\Backups\deploy-10b.log' -Append | Out-Null
try {
$root    = Split-Path -Parent $PSScriptRoot
$dist    = Join-Path $root 'Fino_Web\dist'
$publish = Join-Path $root 'deploy\publish-api-finocore'
$apkNew  = Join-Path $root 'Fino_Android\app\build\outputs\apk\finocore\release\app-finocore-release.apk'
$uiLive  = 'C:\inetpub\wwwroot\ui.finocore.net'
$apiLive = 'C:\inetpub\wwwroot\api.finocore.net'
$appcmd  = "$env:windir\system32\inetsrv\appcmd.exe"
$stamp   = Get-Date -Format 'yyyyMMdd-HHmm'

if (-not (Test-Path (Join-Path $dist 'index.html')))       { throw "No web build at $dist" }
if (-not (Test-Path (Join-Path $publish 'FintechApp.dll'))) { throw "No API publish at $publish" }

Write-Host "== 0. Backup" -ForegroundColor Cyan
$bk = "C:\Backups\finocore-$stamp"
New-Item -ItemType Directory -Force "$bk\ui", "$bk\api" | Out-Null
robocopy $uiLive  "$bk\ui"  /E /NFL /NDL /NJH /NP /R:1 /W:1 | Out-Null
robocopy $apiLive "$bk\api" /E /NFL /NDL /NJH /NP /R:1 /W:1 /XD logs Logs | Out-Null
Write-Host "   -> $bk"

Write-Host "== 1. UI -> $uiLive" -ForegroundColor Cyan
robocopy $dist $uiLive /E /NFL /NDL /NJH /NP /R:2 /W:2 /XF config.json app-config.json web.config favicon.ico | Select-Object -Last 4
if ($LASTEXITCODE -ge 8) { throw "robocopy (UI) failed ($LASTEXITCODE)" }

Write-Host "== 2. app-config.json: blockScreenshots -> false" -ForegroundColor Cyan
$cfgPath = Join-Path $uiLive 'app-config.json'
$cfg = Get-Content $cfgPath -Raw
if ($cfg -match '"blockScreenshots":\s*true') {
    ($cfg -replace '"blockScreenshots":\s*true', '"blockScreenshots": false') | Set-Content $cfgPath -Encoding utf8 -NoNewline
    Write-Host "   updated"
} else { Write-Host "   already off" }

Write-Host "== 3. API -> $apiLive" -ForegroundColor Cyan
& $appcmd stop apppool /apppool.name:"api.finocore.net"
Start-Sleep -Seconds 3
robocopy $publish $apiLive /E /NFL /NDL /NJH /NP /R:5 /W:3 /XF appsettings.Production.json appsettings.Development.json fcm-service-account.json version.json /XD wwwroot Logs logs | Select-Object -Last 4
$rc = $LASTEXITCODE
& $appcmd start apppool /apppool.name:"api.finocore.net"
if ($rc -ge 8) { throw "robocopy (API) failed ($rc) - pool restarted on the old files" }
# Keep the API alive overnight (idempotent; the live pool already has these two settings).
& $appcmd set apppool /apppool.name:"api.finocore.net" /processModel.idleTimeout:"00:00:00" /startMode:"AlwaysRunning" | Out-Null
Write-Host "   app pool: idleTimeout 0, startMode AlwaysRunning"
$a = (Get-FileHash (Join-Path $publish 'FintechApp.dll')).Hash
$b = (Get-FileHash (Join-Path $apiLive 'FintechApp.dll')).Hash
if ($a -ne $b) { throw "FintechApp.dll on the live site does not match the publish." }
$ok = $false
for ($i = 0; $i -lt 20 -and -not $ok; $i++) {
    try { $h = Invoke-RestMethod -Uri 'https://api.finocore.net/health' -TimeoutSec 10; if ($h.status -eq 'ok') { $ok = $true; Write-Host "   API health: $($h | ConvertTo-Json -Compress)" } }
    catch { Start-Sleep -Seconds 3 }
}
if (-not $ok) { throw "API did not answer /health with status ok within 60 s." }

Write-Host "== 4. APK" -ForegroundColor Cyan
if (-not (Test-Path $apkNew)) {
    Write-Warning "No fresh APK at $apkNew - build it first (gradlew assembleFinocoreRelease). Skipping the APK step."
} else {
    $apkLive = Join-Path $uiLive 'app\finocore.apk'
    $apksigner = Get-ChildItem 'C:\buildtools\android-sdk\build-tools' -Recurse -Filter apksigner.bat | Select-Object -First 1
    if ($apksigner -and (Test-Path $apkLive)) {
        $certNew  = (& $apksigner.FullName verify --print-certs $apkNew  2>$null | Select-String 'SHA-256').Line
        $certLive = (& $apksigner.FullName verify --print-certs $apkLive 2>$null | Select-String 'SHA-256').Line
        if ($certNew -ne $certLive) { throw "The new APK is signed with a DIFFERENT key than the one on the site. Not copied.`n new : $certNew`n live: $certLive" }
        Write-Host "   signing key matches the APK already on the site"
    }
    New-Item -ItemType Directory -Force (Split-Path $apkLive) | Out-Null
    Copy-Item $apkNew $apkLive -Force
    Copy-Item $apkNew (Join-Path $root 'deploy\prebuilt-apk\finocore.apk') -Force
    Write-Host "   copied $(((Get-Item $apkNew).Length/1MB).ToString('0.0')) MB -> $apkLive"
}

Write-Host "Done. Open https://finocore.net and hard-refresh (Ctrl+F5). Android users get the update from https://finocore.net/app/finocore.apk; the installed app allows screenshots after its next launch." -ForegroundColor Green
}
catch {
    Write-Host "" ; Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
}
finally {
    Stop-Transcript | Out-Null
    Read-Host 'Press Enter to close this window' | Out-Null
}
