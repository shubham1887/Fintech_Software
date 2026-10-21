<#
.SYNOPSIS
    Export the seeded tax masters so Verify can run the engine against them.
.DESCRIPTION
    The verification project references only FintechApp.dll - no NuGet packages - so it cannot
    talk to SQL Server itself. This script dumps the four masters plus the service catalogue to
    pipe-separated files the project reads. Keeping it this way is deliberate: mixing the
    project's own package graph with the API's build output produced unresolvable assembly
    version conflicts, and this machine cannot always reach nuget.org.
.EXAMPLE
    .\Export-Masters.ps1 -Database FintechApp
#>
[CmdletBinding()]
param(
    [string] $Server   = "localhost\SQLEXPRESS",
    [Parameter(Mandatory = $true)] [string] $Database,
    [string] $OutDir   = ""
)
$ErrorActionPreference = "Stop"
# $PSScriptRoot is empty when the script is piped or dot-sourced rather than run with -File, so
# resolve the script's own folder here instead of in the parameter default.
if ([string]::IsNullOrWhiteSpace($OutDir)) {
    $root = $PSScriptRoot
    if ([string]::IsNullOrWhiteSpace($root)) { $root = Split-Path -Parent $MyInvocation.MyCommand.Path }
    if ([string]::IsNullOrWhiteSpace($root)) { $root = (Get-Location).Path }
    $OutDir = Join-Path $root "data"
}
$sqlcmd = (Get-Command sqlcmd -ErrorAction SilentlyContinue).Source
if ($null -eq $sqlcmd) {
    $sqlcmd = (Get-ChildItem "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC" -Filter sqlcmd.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1).FullName
}
if ($null -eq $sqlcmd) { throw "sqlcmd not found." }
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }

$queries = @{
    "rules.psv"    = "SELECT TaxRuleMasterID,Direction,ServiceID,OperatorID,ProviderID,UserTypeID,PartyUserID,ISNULL(NULLIF(PartyRegistrationStatus,''),'-'),ISNULL(NULLIF(SupplyNature,''),'-'),BillingBasis,ISNULL(TaxRateMasterID,0),ISNULL(HSNSACMasterID,0),ISNULL(TDSSectionMasterID,0),CAST(ReverseCharge AS int),CAST(ITCEligible AS int),Priority,CONVERT(varchar(10),EffectiveFrom,120),ISNULL(CONVERT(varchar(10),EffectiveTo,120),'-'),CAST(IsActive AS int),RuleName FROM dbo.TaxRuleMasters WHERE IsDeleted=0;"
    "rates.psv"    = "SELECT TaxRateMasterID,TaxType,Code,Rate,CessRate,CONVERT(varchar(10),EffectiveFrom,120),ISNULL(CONVERT(varchar(10),EffectiveTo,120),'-'),CAST(IsActive AS int) FROM dbo.TaxRateMasters WHERE IsDeleted=0;"
    "sacs.psv"     = "SELECT HSNSACMasterID,Code,CONVERT(varchar(10),EffectiveFrom,120),ISNULL(CONVERT(varchar(10),EffectiveTo,120),'-') FROM dbo.HSNSACMasters WHERE IsDeleted=0;"
    "sections.psv" = "SELECT TDSSectionMasterID,SectionCode,Rate,ThresholdAnnual,NoPANRate,NonFilerRate,CONVERT(varchar(10),EffectiveFrom,120),ISNULL(CONVERT(varchar(10),EffectiveTo,120),'-') FROM dbo.TDSSectionMasters WHERE IsDeleted=0;"
    "services.psv" = "SELECT ServiceID,ISNULL(NULLIF(ServiceCategory,''),'-'),Name FROM dbo.ServiceMasters WHERE IsDeleted=0;"
}
foreach ($file in $queries.Keys) {
    $path = Join-Path $OutDir $file
    & $sqlcmd -S $Server -E -C -d $Database -h-1 -W -s"|" -Q ("SET NOCOUNT ON; " + $queries[$file]) |
        ForEach-Object { ($_ -replace '\s*\|\s*', '|') } |
        Where-Object { $_.Trim().Length -gt 0 } |
        Set-Content -Path $path -Encoding utf8
    Write-Host ("  {0,-14} {1} rows" -f $file, (Get-Content $path | Measure-Object -Line).Lines)
}
Write-Host "Exported to $OutDir"
