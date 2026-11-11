<#
.SYNOPSIS
    Regenerates Fino_API/Scripts/install/10_schema.sql from the current EF Core model.

.DESCRIPTION
    This is a DEVELOPER tool, not part of a customer installation. Run it whenever entities
    change, and commit the result; the installer then only has to run plain SQL, which means the
    customer's server never needs the .NET SDK or the EF tooling.

    What it does:
      1. dotnet ef dbcontext script  -> the exact schema the current entities describe
         (the migration chain is NOT used: the model snapshot in this repo has drifted from the
         entities, so a migrations-only schema would be wrong).
      2. Wraps every statement in an existence check, so the result is add-only and re-runnable:
         a missing table is created, an existing one is left exactly as it is.
      3. Stamps __EFMigrationsHistory with every migration id in Fino_API/Migrations, so a future
         `dotnet ef database update` against a freshly installed database is a no-op rather than an
         attempt to create everything a second time.

.EXAMPLE
    pwsh ./deploy/tools/New-SchemaScript.ps1
#>
[CmdletBinding()]
param(
    [string] $ApiProject = '',
    [string] $OutFile    = ''
)

$ErrorActionPreference = 'Stop'

# $PSScriptRoot is not reliably populated inside a param() default on Windows PowerShell 5.1,
# so the paths are resolved here instead.
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot   = (Resolve-Path (Join-Path $scriptRoot '..\..')).Path
if (-not $ApiProject) { $ApiProject = Join-Path $repoRoot 'Fino_API' }
if (-not $OutFile)    { $OutFile    = Join-Path $repoRoot 'Fino_API\Scripts\install\10_schema.sql' }
$ApiProject = (Resolve-Path $ApiProject).Path

Write-Host 'Generating schema from the EF model...' -ForegroundColor Cyan
$raw = Join-Path ([System.IO.Path]::GetTempPath()) ("fino_model_{0}.sql" -f [guid]::NewGuid().ToString('N'))

Push-Location $ApiProject
try {
    & dotnet ef dbcontext script -o $raw | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef dbcontext script failed with exit code $LASTEXITCODE." }
} finally {
    Pop-Location
}

$sql = Get-Content -LiteralPath $raw -Raw -Encoding UTF8
Remove-Item -LiteralPath $raw -Force

# Statements are separated by a line containing only GO.
$statements = [regex]::Split($sql, '(?im)^\s*GO\s*$') |
              ForEach-Object { $_.Trim() } |
              Where-Object { $_.Length -gt 0 }

$out    = [System.Text.StringBuilder]::new()
$counts = @{ table = 0; index = 0; passthrough = 0 }

function Add-Guarded {
    param([string] $Guard, [string] $Body)
    [void]$out.AppendLine("IF $Guard")
    [void]$out.AppendLine('BEGIN')
    [void]$out.AppendLine($Body)
    [void]$out.AppendLine('END;')
    [void]$out.AppendLine('GO')
    [void]$out.AppendLine()
}

foreach ($stmt in $statements) {
    $clean = $stmt.TrimStart([char]0xFEFF)

    if ($clean -match '^CREATE TABLE \[([^\]]+)\]') {
        Add-Guarded "OBJECT_ID(N'[$($Matches[1])]', N'U') IS NULL" $clean
        $counts.table++
        continue
    }

    if ($clean -match '^CREATE (?:UNIQUE )?INDEX \[([^\]]+)\] ON \[([^\]]+)\]') {
        $ix = $Matches[1]; $tbl = $Matches[2]
        Add-Guarded "NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'$ix' AND object_id = OBJECT_ID(N'[$tbl]'))" $clean
        $counts.index++
        continue
    }

    # Anything unrecognised is kept verbatim rather than silently dropped.
    [void]$out.AppendLine($clean)
    [void]$out.AppendLine('GO')
    [void]$out.AppendLine()
    $counts.passthrough++
}

# Stamp the migration history so EF considers this database up to date.
$migrationsDir = Join-Path $ApiProject 'Migrations'
$migrationIds = Get-ChildItem -LiteralPath $migrationsDir -Filter '*.cs' |
                Where-Object { $_.Name -notlike '*.Designer.cs' -and $_.Name -notlike '*ModelSnapshot.cs' } |
                ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_.Name) } |
                Sort-Object

$efVersion = '9.0.10'

[void]$out.AppendLine(@"
-------------------------------------------------------------------------------
-- EF migration history.
-- The schema above came from the model, not from the migration chain, so the
-- history is stamped here: a later `dotnet ef database update` then has nothing
-- to replay against a freshly installed database.
-------------------------------------------------------------------------------
IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO
"@)

foreach ($id in $migrationIds) {
    [void]$out.AppendLine("IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'$id')")
    [void]$out.AppendLine('BEGIN')
    [void]$out.AppendLine("    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'$id', N'$efVersion');")
    [void]$out.AppendLine('END;')
    [void]$out.AppendLine('GO')
}

$header = @"
/* ============================================================================
   Fino - application schema (ADD-ONLY, idempotent).

   GENERATED FILE - do not edit by hand.
   Regenerate with:  pwsh ./deploy/tools/New-SchemaScript.ps1

   Generated from the EF Core model on $(Get-Date -Format 'yyyy-MM-dd HH:mm') .
   Every object is created only if it is missing, so running this against an
   existing database adds what that database lacks and changes nothing else.
   It never drops or alters an existing column, so it cannot lose data.

   Applied automatically at API startup by Helpers/DatabaseBootstrapper.cs.
   ============================================================================ */

"@

$dir = Split-Path -Parent $OutFile
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
$content = $header + $out.ToString()
[System.IO.File]::WriteAllText($OutFile, $content, [System.Text.UTF8Encoding]::new($false))

Write-Host ("Wrote {0}" -f (Resolve-Path $OutFile).Path) -ForegroundColor Green
Write-Host ("  tables: {0}   indexes: {1}   passthrough: {2}   migrations stamped: {3}" -f `
    $counts.table, $counts.index, $counts.passthrough, $migrationIds.Count)
