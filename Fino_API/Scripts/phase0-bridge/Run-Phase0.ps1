<#
.SYNOPSIS
    Phase 0 Gross-to-Taxable Bridge -- saare read-only reports chalata hai
    aur CSV export karta hai, taaki seedha CA ko bheja ja sake.

.DESCRIPTION
    Yeh script sirf SELECT chalata hai. Koi INSERT / UPDATE / DELETE / DDL
    nahi. Live database par safe hai (READ UNCOMMITTED), par phir bhi
    behtar yeh hai ki pehle ek backup copy par chalao.

.EXAMPLE
    .\Run-Phase0.ps1 -Database finocore_db -From 2025-04-01 -To 2026-03-31

.EXAMPLE
    .\Run-Phase0.ps1 -Server "SQLPROD\INST1" -Database finocore_db `
                     -From 2025-04-01 -To 2026-03-31 -OutDir D:\phase0-out
#>
[CmdletBinding()]
param(
    [string] $Server   = "localhost\SQLEXPRESS",
    [Parameter(Mandatory = $true)]
    [string] $Database,
    [string] $From     = "2025-04-01",
    [string] $To       = "2026-03-31",
    [string] $OutDir   = "",
    [string] $UserName = "",
    [string] $Password = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

if ([string]::IsNullOrWhiteSpace($OutDir)) {
    $OutDir = Join-Path $scriptDir ("out-" + (Get-Date -Format "yyyyMMdd-HHmmss"))
}
if (-not (Test-Path $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
}

# --- sqlcmd dhoondho -------------------------------------------------
$sqlcmd = $null
$cmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
if ($null -ne $cmd) {
    $sqlcmd = $cmd.Source
} else {
    $candidates = Get-ChildItem -Path "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC" `
                                -Filter sqlcmd.exe -Recurse -ErrorAction SilentlyContinue
    if ($candidates.Count -gt 0) { $sqlcmd = $candidates[0].FullName }
}
if ($null -eq $sqlcmd) {
    throw "sqlcmd nahi mila. SQL Server command line tools install karo, ya scripts ko SSMS mein chalao."
}

Write-Host "Phase 0 Bridge" -ForegroundColor Cyan
Write-Host ("  Server   : {0}" -f $Server)
Write-Host ("  Database : {0}" -f $Database)
Write-Host ("  Period   : {0} se {1}" -f $From, $To)
Write-Host ("  Output   : {0}" -f $OutDir)
Write-Host ""

# --- auth args -------------------------------------------------------
$authArgs = @()
if ([string]::IsNullOrWhiteSpace($UserName)) {
    $authArgs = @("-E")
} else {
    $authArgs = @("-U", $UserName, "-P", $Password)
}

$scripts = @(
    @{ File = "00_provider_wallet_closing.sql";   Title = "Provider wallet closing balance (1 crore ka core test)" },
    @{ File = "01_gross_to_taxable_bridge.sql";   Title = "Gross-to-Taxable Bridge" },
    @{ File = "02_exchange_partner_register.sql"; Title = "Exchange partner register" },
    @{ File = "03_non_gst_party_register.sql";    Title = "Non-GST party register + 194H tracker" },
    @{ File = "04_data_quality_checks.sql";       Title = "Data quality checks" }
)

$failed = 0

foreach ($s in $scripts) {
    $src = Join-Path $scriptDir $s.File
    if (-not (Test-Path $src)) {
        Write-Host ("SKIP  {0} -- file nahi mili" -f $s.File) -ForegroundColor Yellow
        continue
    }

    # @From / @To ko requested period par set karo (original file nahi badalta)
    $tmp  = Join-Path $OutDir ("_run_" + $s.File)
    $text = Get-Content -Path $src -Raw
    $text = [regex]::Replace($text, "@From\s+date\s*=\s*'[^']*'", ("@From date = '{0}'" -f $From))
    $text = [regex]::Replace($text, "@To\s+date\s*=\s*'[^']*'",   ("@To   date = '{0}'" -f $To))
    Set-Content -Path $tmp -Value $text -Encoding utf8

    $csv = Join-Path $OutDir ([System.IO.Path]::ChangeExtension($s.File, "csv"))

    Write-Host ("RUN   {0}" -f $s.Title) -ForegroundColor White
    $sqlArgs = $authArgs + @(
        "-S", $Server, "-d", $Database, "-C",
        "-i", $tmp, "-o", $csv,
        "-W", "-s", ",", "-w", "65535"
    )
    & $sqlcmd @sqlArgs
    $code = $LASTEXITCODE

    Remove-Item -Path $tmp -Force -ErrorAction SilentlyContinue

    $errLines = @()
    if (Test-Path $csv) {
        $errLines = Select-String -Path $csv -Pattern "^Msg \d+, Level" -ErrorAction SilentlyContinue
    }

    if ($code -ne 0 -or $errLines.Count -gt 0) {
        Write-Host ("      FAIL -- {0} dekho" -f $csv) -ForegroundColor Red
        foreach ($e in ($errLines | Select-Object -First 3)) { Write-Host ("      " + $e.Line) -ForegroundColor Red }
        $failed = $failed + 1
    } else {
        Write-Host ("      OK   -> {0}" -f $csv) -ForegroundColor Green
    }
}

Write-Host ""
if ($failed -gt 0) {
    Write-Host ("{0} script fail hui. Upar ke CSV mein error dekho." -f $failed) -ForegroundColor Red
} else {
    Write-Host "Sab reports ban gayi." -ForegroundColor Green
    Write-Host ""
    Write-Host "CA ko sabse pehle yeh dena hai:" -ForegroundColor Cyan
    Write-Host "  1. 00_provider_wallet_closing.csv -- section A-TOTAL ka GrandTotalClosingStock"
    Write-Host "  2. 01_gross_to_taxable_bridge.csv -- section C ka UnexplainedGap"
    Write-Host "  3. 02_exchange_partner_register.csv -- section B ka TotalTurnoverMissed"
}
Write-Host ""
Write-Host ("Output folder: {0}" -f $OutDir)
