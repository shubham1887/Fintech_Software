<#
    Deploys the 10 Nov 2026 issue fixes to the live finocore sites on this box.
    Run from an elevated PowerShell:  powershell -ExecutionPolicy Bypass -File .\deploy\Deploy-IssueFixes-2026-11-10.ps1

    What it does, in order:
      1. Applies Fino_API\Scripts\patch_2026-11-10_issue_fixes.sql to finocore_db (idempotent).
      2. Copies the vite build (Fino_Web\dist) over C:\inetpub\wwwroot\ui.finocore.net, keeping the
         live config.json / app-config.json / web.config / favicon.ico and every older hashed asset.
      3. Stops app pool api.finocore.net, copies deploy\publish-api-finocore over
         C:\inetpub\wwwroot\api.finocore.net (keeping appsettings.Production.json, fcm-service-account.json,
         version.json, wwwroot, Logs), starts the pool, waits for /health.
      4. Proves the API deploy landed by comparing FintechApp.dll hashes.
    Both live folders were already backed up to C:\Backups\finocore-20261110-2107.
#>
$ErrorActionPreference = 'Stop'
$root    = Split-Path -Parent $PSScriptRoot
$dist    = Join-Path $root 'Fino_Web\dist'
$publish = Join-Path $root 'deploy\publish-api-finocore'
$uiLive  = 'C:\inetpub\wwwroot\ui.finocore.net'
$apiLive = 'C:\inetpub\wwwroot\api.finocore.net'
$appcmd  = "$env:windir\system32\inetsrv\appcmd.exe"

if (-not (Test-Path (Join-Path $dist 'index.html')))    { throw "No web build at $dist - run 'npm run build' in Fino_Web first." }
if (-not (Test-Path (Join-Path $publish 'FintechApp.dll'))) { throw "No API publish at $publish - run the dotnet publish first." }

Write-Host "== 1. Database patch" -ForegroundColor Cyan
sqlcmd -S 'localhost\SQLEXPRESS' -d finocore_db -E -C -I -W -i (Join-Path $root 'Fino_API\Scripts\patch_2026-11-10_issue_fixes.sql')
if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed ($LASTEXITCODE)" }

Write-Host "== 2. UI -> $uiLive" -ForegroundColor Cyan
robocopy $dist $uiLive /E /NFL /NDL /NJH /NP /R:2 /W:2 /XF config.json app-config.json web.config favicon.ico | Select-Object -Last 6
if ($LASTEXITCODE -ge 8) { throw "robocopy (UI) failed ($LASTEXITCODE)" }

Write-Host "== 3. API -> $apiLive" -ForegroundColor Cyan
& $appcmd stop apppool /apppool.name:"api.finocore.net"
Start-Sleep -Seconds 3
robocopy $publish $apiLive /E /NFL /NDL /NJH /NP /R:5 /W:3 /XF appsettings.Production.json appsettings.Development.json fcm-service-account.json version.json /XD wwwroot Logs logs | Select-Object -Last 6
$rc = $LASTEXITCODE
& $appcmd start apppool /apppool.name:"api.finocore.net"
if ($rc -ge 8) { throw "robocopy (API) failed ($rc) - pool restarted on the old files" }

Write-Host "== 4. Verify" -ForegroundColor Cyan
$a = (Get-FileHash (Join-Path $publish 'FintechApp.dll')).Hash
$b = (Get-FileHash (Join-Path $apiLive 'FintechApp.dll')).Hash
if ($a -ne $b) { throw "FintechApp.dll on the live site does not match the publish - the copy did not land." }
Write-Host "FintechApp.dll matches the publish ($b)"

$ok = $false
for ($i = 0; $i -lt 20 -and -not $ok; $i++) {
    try {
        $h = Invoke-RestMethod -Uri 'https://api.finocore.net/health' -TimeoutSec 10
        if ($h.status -eq 'ok') { $ok = $true; Write-Host "API health: $($h | ConvertTo-Json -Compress)" }
    } catch { Start-Sleep -Seconds 3 }
}
if (-not $ok) { throw "API did not answer /health with status ok within 60 s - check the app pool and Logs." }

$index = Get-Content (Join-Path $uiLive 'index.html') -Raw
$assets = [regex]::Matches($index, '/assets/[^"'']+') | ForEach-Object { $_.Value } | Sort-Object -Unique
foreach ($p in $assets) {
    if (-not (Test-Path (Join-Path $uiLive $p.TrimStart('/')))) { throw "index.html references $p but it is not on disk." }
}
Write-Host "UI: index.html and $($assets.Count) hashed assets are in place."
Write-Host "Done. Open https://finocore.net and hard-refresh (Ctrl+F5)." -ForegroundColor Green
