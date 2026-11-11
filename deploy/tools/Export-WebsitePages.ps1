<#
.SYNOPSIS
    Regenerates Fino_API/Scripts/install/43_seed_website_pages.sql from a reference database.

.DESCRIPTION
    DEVELOPER tool. The public marketing site (Home / Services / About / Contact) is stored as rows
    in WebsitePageMasters, so a brand-new installation would otherwise come up with an empty public
    site. This dumps those rows from a known-good database into an idempotent seed that ships with
    the installer; the client then edits the copy from the panel's Website Builder.

    Only page content is exported - no user, transaction or credential data.

.EXAMPLE
    pwsh ./deploy/tools/Export-WebsitePages.ps1 -Server ".\SQLEXPRESS" -Database "FintechApp"
#>
[CmdletBinding()]
param(
    [string] $Server   = '.\SQLEXPRESS',
    [string] $Database = 'FintechApp',
    [string] $OutFile  = ''
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot   = (Resolve-Path (Join-Path $scriptRoot '..\..')).Path
if (-not $OutFile) { $OutFile = Join-Path $repoRoot 'Fino_API\Scripts\install\43_seed_website_pages.sql' }

$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
$conn.Open()

try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @'
SELECT Slug, Title, MetaDescription, SectionsJson, MenuOrder, ShowInMenu, IsPublished
FROM WebsitePageMasters
WHERE ISNULL(IsDeleted, 0) = 0
ORDER BY MenuOrder, WebsitePageMasterID
'@
    $reader = $cmd.ExecuteReader()

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine(@"
/* ============================================================================
   Public website - default pages (Home / Services / About / Contact).

   GENERATED FILE - do not edit by hand.
   Regenerate with:  pwsh ./deploy/tools/Export-WebsitePages.ps1

   The public site is data, not code: without these rows a new installation
   serves an empty website. Each page is inserted only when its slug is absent,
   so this never overwrites content a client has already edited in
   Settings -> Website.
   ============================================================================ */

SET NOCOUNT ON;
"@)

    $count = 0
    while ($reader.Read()) {
        function Q([object] $v) {
            if ($null -eq $v -or $v -is [DBNull]) { return "N''" }
            return "N'" + ($v.ToString() -replace "'", "''") + "'"
        }

        $slug     = $reader['Slug']
        $title    = Q $reader['Title']
        $meta     = Q $reader['MetaDescription']
        $sections = Q $reader['SectionsJson']
        $order    = [int]$reader['MenuOrder']
        $inMenu   = if ([bool]$reader['ShowInMenu'])  { 1 } else { 0 }
        $pub      = if ([bool]$reader['IsPublished']) { 1 } else { 0 }
        $slugSql  = Q $slug

        [void]$sb.AppendLine("")
        [void]$sb.AppendLine("-- $slug")
        [void]$sb.AppendLine("IF NOT EXISTS (SELECT 1 FROM WebsitePageMasters WHERE Slug = $slugSql)")
        [void]$sb.AppendLine("BEGIN")
        [void]$sb.AppendLine("    INSERT INTO WebsitePageMasters")
        [void]$sb.AppendLine("        (Slug, Title, MetaDescription, SectionsJson, MenuOrder, ShowInMenu, IsPublished, [DateTime], UpdatedateTime, IsDeleted)")
        [void]$sb.AppendLine("    VALUES")
        [void]$sb.AppendLine("        ($slugSql, $title, $meta,")
        [void]$sb.AppendLine("         $sections,")
        [void]$sb.AppendLine("         $order, $inMenu, $pub, GETDATE(), GETDATE(), 0);")
        [void]$sb.AppendLine("END;")
        [void]$sb.AppendLine("GO")
        $count++
    }
    $reader.Close()

    [System.IO.File]::WriteAllText($OutFile, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
    Write-Host ("Wrote {0} page(s) to {1}" -f $count, $OutFile) -ForegroundColor Green
}
finally {
    $conn.Close()
}
