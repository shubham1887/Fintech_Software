<#
    Shared helpers for the Fino setup scripts.

    Dot-source it:  . "$scriptRoot\lib\FinoCommon.ps1"

    Everything the installer, the updater and the branding tool all need lives here, so that the
    three of them cannot drift apart - a prompt that behaves differently in the updater than in the
    installer is exactly the kind of small inconsistency that confuses whoever is running it.

    ASCII only, deliberately: Windows PowerShell 5.1 reads a BOM-less .ps1 as ANSI, so a stray em
    dash turns into mojibake and the parser falls over.
#>

# ---------------------------------------------------------------------------
# Console
# ---------------------------------------------------------------------------
function Write-Step  ([string] $t) { Write-Host "`n=== $t" -ForegroundColor Cyan }
function Write-Ok    ([string] $t) { Write-Host "  [OK]   $t" -ForegroundColor Green }
function Write-Warn2 ([string] $t) { Write-Host "  [!]    $t" -ForegroundColor Yellow }
function Write-Fail  ([string] $t) { Write-Host "  [FAIL] $t" -ForegroundColor Red }
function Write-Note  ([string] $t) { Write-Host "  [ ]    $t" -ForegroundColor DarkGray }

function Write-Banner {
    param([string] $Title, [string] $Subtitle)
    Write-Host ''
    Write-Host '  ============================================================' -ForegroundColor White
    Write-Host "    $Title" -ForegroundColor White
    if ($Subtitle) { Write-Host "    $Subtitle" -ForegroundColor DarkGray }
    Write-Host '  ============================================================' -ForegroundColor White
}

<#
    Reads one answer from the console, or reports that there is no console.

    PowerShell started with -NonInteractive - which is how the graphical setup and any scheduled
    run start it - does not return an empty answer from Read-Host: it throws. So every prompt goes
    through here, and a host that cannot be asked is a fact the callers handle by falling back to
    their default, rather than an exception three frames up.
#>
$script:PromptingUnavailable = $false

function Read-Answer {
    param([string] $Prompt, [switch] $Secret)

    if ($script:PromptingUnavailable) { return $null }

    try {
        if ($Secret) {
            $secure = Read-Host $Prompt -AsSecureString
            return [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
        }
        return Read-Host $Prompt
    } catch {
        $script:PromptingUnavailable = $true
        return $null
    }
}

function Ask {
    param([string] $Question, [string] $Default, [switch] $Required, [switch] $Secret)

    while ($true) {
        $suffix = if ($Default) { " [$Default]" } else { '' }
        $value = Read-Answer "$Question$suffix" -Secret:$Secret

        if (-not $value) { $value = $Default }
        if ($value -or -not $Required) { return $value }

        if ($script:PromptingUnavailable) {
            # Looping would spin forever with nobody to answer. Say which answer is missing and how
            # to supply it, because the caller is a script or a program, not a person.
            throw ("No answer for '{0}' and nothing to ask - pass it as a parameter when running this without a console." -f $Question.Trim())
        }
        Write-Warn2 'This one is required.'
    }
}

function Ask-YesNo {
    param([string] $Question, [bool] $Default = $true)

    $hint = if ($Default) { 'Y/n' } else { 'y/N' }
    while ($true) {
        $answer = Read-Answer "$Question [$hint]"
        if ($script:PromptingUnavailable) { return $Default }

        $answer = ([string]$answer).Trim().ToLower()
        if (-not $answer) { return $Default }
        if ($answer -in @('y', 'yes')) { return $true }
        if ($answer -in @('n', 'no'))  { return $false }
        Write-Warn2 'Please answer y or n.'
    }
}

function Test-IsAdministrator {
    ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# ---------------------------------------------------------------------------
# Secrets
# ---------------------------------------------------------------------------
function New-Secret {
    param([int] $Length)
    # Alphanumeric only: these values end up in JSON config files, in a service's environment and
    # (for the AES key) in a browser-readable config.json, so punctuation buys nothing and breaks
    # quoting somewhere down the line.
    $alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'
    $bytes = [byte[]]::new($Length)
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    -join ($bytes | ForEach-Object { $alphabet[$_ % $alphabet.Length] })
}

function New-SqlPassword {
    # SQL Server refuses a password that does not mix character classes, so the classes are added
    # rather than hoped for.
    (New-Secret 20) + 'aA1#'
}

# ---------------------------------------------------------------------------
# SQL
# ---------------------------------------------------------------------------
function Get-SanitisedConnectionString {
    param([string] $ConnectionString)
    # Windows PowerShell ships the .NET Framework SqlClient, which rejects keywords only the modern
    # Microsoft.Data.SqlClient understands - "Command Timeout" among them, and the application's
    # connection string has one.
    foreach ($keyword in @('Command Timeout', 'CommandTimeout')) {
        $ConnectionString = [regex]::Replace(
            $ConnectionString, "(?i)\s*$([regex]::Escape($keyword))\s*=\s*[^;]*;?", '')
    }
    $ConnectionString
}

function Invoke-Sql {
    param([string] $ConnectionString, [string] $Sql, [hashtable] $Parameters = @{})
    $conn = New-Object System.Data.SqlClient.SqlConnection (Get-SanitisedConnectionString $ConnectionString)
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $Sql
        $cmd.CommandTimeout = 0
        foreach ($key in $Parameters.Keys) { [void]$cmd.Parameters.AddWithValue($key, $Parameters[$key]) }
        return $cmd.ExecuteScalar()
    } finally {
        $conn.Dispose()
    }
}

function Test-SqlConnection {
    param([string] $ConnectionString)
    $conn = New-Object System.Data.SqlClient.SqlConnection (Get-SanitisedConnectionString $ConnectionString)
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = 'SELECT @@VERSION'
        return @{ ok = $true; version = ($cmd.ExecuteScalar() -split "`n")[0].Trim() }
    } catch {
        return @{ ok = $false; error = $_.Exception.Message }
    } finally {
        $conn.Dispose()
    }
}

<#
    Backs up a database to $TargetPath and returns where the file actually ended up.

    BACKUP DATABASE runs inside SQL Server, as the SQL Server service account - not as whoever is
    running this script. That account frequently cannot write to a user folder or a network share,
    which is why the fallback exists: write to the instance's own backup folder, which it can always
    write to, then move the file with our own rights.

    COMPRESSION is not available on SQL Server Express, so a failure mentioning it is retried
    without. Skipping the backup is never an option here - that is the failure that loses data.
#>
function Invoke-DatabaseBackup {
    param(
        [Parameter(Mandatory = $true)] [string] $ConnectionString,
        [Parameter(Mandatory = $true)] [string] $TargetPath,
        [string] $Label = 'Fino automated backup'
    )

    $database = Get-DatabaseNameFromConnectionString $ConnectionString
    if (-not $database) { throw 'Could not read the database name out of the connection string.' }

    $conn = New-Object System.Data.SqlClient.SqlConnection (Get-SanitisedConnectionString $ConnectionString)
    $conn.Open()
    try {
        function Invoke-BackupTo {
            param([string] $Path, [switch] $NoCompression)
            $cmd = $conn.CreateCommand()
            $cmd.CommandTimeout = 0
            $with = if ($NoCompression) { 'COPY_ONLY, INIT' } else { 'COPY_ONLY, INIT, COMPRESSION' }
            $cmd.CommandText = "BACKUP DATABASE [$database] TO DISK = @path WITH $with, NAME = @label;"
            [void]$cmd.Parameters.AddWithValue('@path', $Path)
            [void]$cmd.Parameters.AddWithValue('@label', $Label)
            $cmd.ExecuteNonQuery() | Out-Null
        }

        $compressed = $true
        $finalPath  = $TargetPath

        try { Invoke-BackupTo -Path $TargetPath } catch {
            if ($_.Exception.Message -match 'COMPRESSION|not supported') {
                $compressed = $false
                Invoke-BackupTo -Path $TargetPath -NoCompression
            } elseif ($_.Exception.Message -match 'Operating system error 5|Access is denied|Cannot open backup device') {
                $pathCmd = $conn.CreateCommand()
                $pathCmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000))"
                $defaultPath = $pathCmd.ExecuteScalar()
                if (-not $defaultPath) { throw 'SQL Server cannot write to that folder and did not report a backup folder of its own.' }

                $serverPath = Join-Path $defaultPath (Split-Path -Leaf $TargetPath)
                try { Invoke-BackupTo -Path $serverPath } catch { $compressed = $false; Invoke-BackupTo -Path $serverPath -NoCompression }

                if (Test-Path $serverPath) {
                    Move-Item $serverPath $TargetPath -Force
                } else {
                    # A remote SQL Server: the file is on that machine. Saying so is more useful than
                    # pretending the backup is here.
                    $finalPath = $serverPath
                }
            } else {
                throw
            }
        }

        return [pscustomobject]@{
            Path       = $finalPath
            Compressed = $compressed
            OnThisHost = (Test-Path $finalPath)
        }
    } finally {
        $conn.Dispose()
    }
}

function Get-DatabaseNameFromConnectionString {
    param([string] $ConnectionString)
    if ($ConnectionString -match '(?i)(?:Initial Catalog|Database)\s*=\s*([^;]+)') { return $Matches[1].Trim() }
    return $null
}

function Get-SqlServerFromConnectionString {
    param([string] $ConnectionString)
    if ($ConnectionString -match '(?i)(?:Data Source|Server)\s*=\s*([^;]+)') { return $Matches[1].Trim() }
    return $null
}

# ---------------------------------------------------------------------------
# Ports
#
# This exists because of a real incident: 1433 was entered as the application's port during an
# install. 1433 is SQL Server's port, so the service could never bind it and crashed on every
# start - including on the rollback, because the port lives in the configuration, which an upgrade
# correctly preserves. The install had written a configuration that could not work.
#
# So the port is now checked before anything is written: a port that belongs to a critical Windows
# service is refused outright, and a port something else is already listening on is refused with
# the name of the program holding it.
# ---------------------------------------------------------------------------
$script:ReservedPorts = @{
    53    = 'DNS'
    88    = 'Kerberos'
    135   = 'Windows RPC'
    139   = 'NetBIOS'
    389   = 'Active Directory (LDAP)'
    445   = 'Windows file sharing'
    1433  = 'SQL Server'
    1434  = 'SQL Server Browser'
    3306  = 'MySQL'
    3389  = 'Remote Desktop - taking this port locks you out of the server'
    5432  = 'PostgreSQL'
    5985  = 'Windows Remote Management'
    5986  = 'Windows Remote Management (HTTPS)'
    6379  = 'Redis'
    27017 = 'MongoDB'
}

function Get-PortOwner {
    param([int] $Port)
    $connection = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $connection) { return $null }
    $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
    if ($process) { return $process.ProcessName }
    return 'another program'
}

<#
    Decides whether a port can be used for the application.

    Returns @{ ok = $true/$false; reason = '...'; }
#>
function Test-InstallPort {
    param([int] $Port, [string] $IgnoreServiceName)

    if ($Port -lt 1 -or $Port -gt 65535) {
        return @{ ok = $false; reason = "$Port is not a port number - it has to be between 1 and 65535." }
    }

    if ($script:ReservedPorts.ContainsKey($Port)) {
        return @{ ok = $false; reason = "Port $Port belongs to $($script:ReservedPorts[$Port]). Pick another one - 8080, 8081 and 9090 are the usual choices." }
    }

    $owner = Get-PortOwner $Port
    if ($owner) {
        # A re-install or upgrade of our own service legitimately finds its own port in use.
        if ($IgnoreServiceName) {
            $service = Get-CimInstance Win32_Service -Filter "Name='$IgnoreServiceName'" -ErrorAction SilentlyContinue
            if ($service -and $service.ProcessId) {
                $ours = Get-Process -Id $service.ProcessId -ErrorAction SilentlyContinue
                if ($ours -and $ours.ProcessName -eq $owner) { return @{ ok = $true } }
            }
        }
        return @{ ok = $false; reason = "Port $Port is already being used by '$owner'. Pick another one." }
    }

    return @{ ok = $true }
}

# ---------------------------------------------------------------------------
# Why a service would not start
#
# "The new build did not come up" is not a diagnosis. The reason is in the Application event log
# within seconds of the attempt, so it is fetched and shown rather than left for someone to find.
# ---------------------------------------------------------------------------
function Get-LastApplicationFault {
    param([int] $WithinMinutes = 15)

    try {
        $entries = Get-EventLog -LogName Application -EntryType Error -Newest 60 `
                       -After (Get-Date).AddMinutes(-$WithinMinutes) -ErrorAction Stop |
                   Where-Object { $_.Message -match 'FintechApp' }
    } catch {
        return $null
    }

    foreach ($entry in $entries) {
        # The useful sentence is the exception message. "Exception code: 0xe0434352" and the stack
        # trace under it say only "a .NET exception happened", which is not worth showing anyone.
        $interesting = ($entry.Message -split "`n" |
            Where-Object {
                $_ -match 'Failed to bind|already in use|Login failed|Cannot open|access denied|permission denied|System\.[A-Za-z.]+Exception|Exception Info' -and
                $_ -notmatch 'Exception code|Faulting|^\s*at\s'
            } |
            ForEach-Object { $_.Trim() } |
            Select-Object -First 2) -join '  '

        if ($interesting) {
            # Long .NET messages carry an "--->" chain; the first link is the one that matters.
            $interesting = ($interesting -split '--->')[0].Trim()
            return $interesting -replace '^Exception Info:\s*', ''
        }
    }
    return $null
}

# ---------------------------------------------------------------------------
# Versions
# ---------------------------------------------------------------------------

<#
    Release versions look like 2026.09.16.1430 (or 2026.09.1). Comparing them as text gets "2026.9"
    and "2026.10" the wrong way round, so each dotted part is compared as a number. Anything that
    is not numeric falls back to a plain string comparison rather than throwing - a hand-labelled
    build should not stop an upgrade.

    Returns -1 when A is older, 0 when equal, 1 when A is newer.
#>
function Compare-FinoVersion {
    param([string] $VersionA, [string] $VersionB)

    $partsA = ($VersionA -split '[.\-]')
    $partsB = ($VersionB -split '[.\-]')
    $count  = [Math]::Max($partsA.Count, $partsB.Count)

    for ($i = 0; $i -lt $count; $i++) {
        $a = if ($i -lt $partsA.Count) { $partsA[$i] } else { '0' }
        $b = if ($i -lt $partsB.Count) { $partsB[$i] } else { '0' }

        $numA = 0; $numB = 0
        if ([int]::TryParse($a, [ref]$numA) -and [int]::TryParse($b, [ref]$numB)) {
            if ($numA -lt $numB) { return -1 }
            if ($numA -gt $numB) { return 1 }
        } else {
            $text = [string]::Compare($a, $b, $true)
            if ($text -ne 0) { return [Math]::Sign($text) }
        }
    }
    return 0
}

function Get-ReleaseVersion {
    param([string] $AppFolder)
    $versionFile = Join-Path $AppFolder 'version.json'
    if (Test-Path $versionFile) {
        try { return (Get-Content $versionFile -Raw | ConvertFrom-Json).version } catch { }
    }
    return 'unknown'
}

# ---------------------------------------------------------------------------
# Installation state
#
# install-state.json is the installer's memory: what was installed, when, where, and how it is
# hosted. Setup.ps1 reads it to decide between a fresh install and an upgrade, and the updater
# reads it to find the service and the database without asking again.
# ---------------------------------------------------------------------------
function Get-InstallStatePath {
    param([string] $InstallPath)
    Join-Path $InstallPath 'install-state.json'
}

function Read-InstallState {
    param([string] $InstallPath)
    $path = Get-InstallStatePath $InstallPath
    if (-not (Test-Path $path)) { return $null }
    try { return Get-Content $path -Raw | ConvertFrom-Json } catch { return $null }
}

function Write-InstallState {
    param([string] $InstallPath, [hashtable] $State)
    $path = Get-InstallStatePath $InstallPath
    $State['updatedOn'] = (Get-Date).ToString('o')
    # No BOM: this file is also read by other tooling, and a BOM trips up several JSON parsers.
    [System.IO.File]::WriteAllText($path, ($State | ConvertTo-Json -Depth 10), [System.Text.UTF8Encoding]::new($false))
}

# ---------------------------------------------------------------------------
# Finding an existing installation
#
# Three shapes are possible on a customer's server:
#   "service"  - installed by this kit
#   "iis"      - an older hand-made deployment (an IIS site pointing at FintechApp.dll)
#   $null      - a clean server
# An upgrade has to recognise all three, because the whole point is that the person running it does
# not know which one they have.
# ---------------------------------------------------------------------------
function Find-FinoInstallation {
    param([string] $PreferredPath = 'C:\Fino', [string] $ServiceName = 'FinoAPI')

    # 1. An installation this kit made, at the usual place or recorded in a service.
    $state = Read-InstallState $PreferredPath
    if ($state) {
        $service = if ($state.serviceName) { Get-Service -Name $state.serviceName -ErrorAction SilentlyContinue } else { $null }
        return @{
            mode         = $state.hosting
            installPath  = $PreferredPath
            serviceName  = $state.serviceName
            version      = $state.version
            state        = $state
            serviceState = if ($service) { $service.Status.ToString() } else { 'not installed' }
            serviceStart = if ($service) { $service.StartType.ToString() } else { '' }
        }
    }

    # 2. A Fino service under any name, pointing somewhere else.
    $services = Get-CimInstance Win32_Service -ErrorAction SilentlyContinue |
                Where-Object { $_.PathName -match 'FintechApp\.exe' }
    foreach ($service in $services) {
        $exe = ($service.PathName -replace '^"|"$', '')
        $appDir = Split-Path -Parent $exe
        $installPath = Split-Path -Parent $appDir
        $windowsService = Get-Service -Name $service.Name -ErrorAction SilentlyContinue
        return @{
            mode         = 'service'
            installPath  = $installPath
            serviceName  = $service.Name
            version      = Get-ReleaseVersion $appDir
            state        = Read-InstallState $installPath
            serviceState = if ($windowsService) { $windowsService.Status.ToString() } else { 'not installed' }
            serviceStart = if ($windowsService) { $windowsService.StartType.ToString() } else { '' }
        }
    }

    # 3. An IIS site hosting the API - the way these were deployed before this kit existed.
    $iis = Find-IisFinoSite
    if ($iis) {
        return @{
            mode        = 'iis'
            installPath = $iis.physicalPath
            siteName    = $iis.siteName
            appPool     = $iis.appPool
            version     = Get-ReleaseVersion $iis.physicalPath
            state       = $null
        }
    }

    return $null
}

<#
    Parses JSON that has // and /* */ comments in it.

    ASP.NET Core accepts comments in appsettings.json and this project uses them heavily - they are
    the documentation for the settings. Windows PowerShell's ConvertFrom-Json does not, so reading
    an existing deployment's configuration means stripping them first. Strings are tracked so that
    a "//" inside a connection string or a URL is not mistaken for a comment.
#>
function ConvertFrom-JsonC {
    param([Parameter(Mandatory = $true)] [string] $Text)

    $out = [System.Text.StringBuilder]::new()
    $inString = $false
    $escaped = $false
    $i = 0

    while ($i -lt $Text.Length) {
        $ch   = $Text[$i]
        $next = if ($i + 1 -lt $Text.Length) { $Text[$i + 1] } else { [char]0 }

        if ($inString) {
            [void]$out.Append($ch)
            if ($escaped)            { $escaped = $false }
            elseif ($ch -eq '\')     { $escaped = $true }
            elseif ($ch -eq '"')     { $inString = $false }
            $i++
            continue
        }

        if ($ch -eq '"') { $inString = $true; [void]$out.Append($ch); $i++; continue }

        if ($ch -eq '/' -and $next -eq '/') {
            while ($i -lt $Text.Length -and $Text[$i] -ne "`n") { $i++ }
            continue
        }

        if ($ch -eq '/' -and $next -eq '*') {
            $i += 2
            while ($i + 1 -lt $Text.Length -and -not ($Text[$i] -eq '*' -and $Text[$i + 1] -eq '/')) { $i++ }
            $i += 2
            continue
        }

        [void]$out.Append($ch)
        $i++
    }

    # A trailing comma left behind by a commented-out last entry is also something .NET tolerates
    # and ConvertFrom-Json does not.
    $cleaned = [regex]::Replace($out.ToString(), ',(\s*[}\]])', '$1')
    return $cleaned | ConvertFrom-Json
}

<#
    Reads the settings out of an older, hand-made deployment so that a migration can keep them.

    The three that matter:
      * the connection string  - the client's live data is in that database
      * the JWT secret         - keep it and nobody is logged out by the move
      * the AES key and IV     - keep them and the panel keeps talking to the API

    Returns $null when the folder has no readable configuration.
#>
function Get-LegacyDeploymentSettings {
    param([string] $Path)

    foreach ($name in @('appsettings.Production.json', 'appsettings.json')) {
        $file = Join-Path $Path $name
        if (-not (Test-Path $file)) { continue }

        try {
            $json = ConvertFrom-JsonC (Get-Content $file -Raw)
        } catch {
            continue
        }

        $connection = $null
        if ($json.ConnectionStrings) { $connection = $json.ConnectionStrings.DefaultConnection }
        if (-not $connection) { continue }

        return [pscustomobject]@{
            SourceFile       = $file
            ConnectionString = $connection
            SqlServer        = Get-SqlServerFromConnectionString $connection
            DatabaseName     = Get-DatabaseNameFromConnectionString $connection
            JwtSecret        = if ($json.JwtSettings) { $json.JwtSettings.SecretKey } else { $null }
            AesKey           = if ($json.Encryption)  { $json.Encryption.Key } else { $null }
            AesIv            = if ($json.Encryption)  { $json.Encryption.IV } else { $null }
        }
    }
    return $null
}

function Find-IisFinoSite {
    if (-not (Test-Path "$env:SystemRoot\System32\inetsrv\appcmd.exe")) { return $null }

    $appcmd = "$env:SystemRoot\System32\inetsrv\appcmd.exe"
    $sites = & $appcmd list site /xml 2>$null
    if (-not $sites) { return $null }

    try { $xml = [xml]($sites -join "`n") } catch { return $null }

    foreach ($site in $xml.appcmd.SITE) {
        $name = $site.'SITE.NAME'
        $vdirs = & $appcmd list vdir /xml "/app.name:$name/" 2>$null
        if (-not $vdirs) { continue }
        try { $vdirXml = [xml]($vdirs -join "`n") } catch { continue }

        foreach ($vdir in $vdirXml.appcmd.VDIR) {
            $physical = $vdir.physicalPath
            if ($physical -and (Test-Path (Join-Path $physical 'FintechApp.dll'))) {
                $appPool = (& $appcmd list app "$name/" /text:applicationPool 2>$null)
                return @{ siteName = $name; physicalPath = $physical; appPool = $appPool }
            }
        }
    }
    return $null
}

function Set-IisSitePerformance {
    <#
        Makes the two IIS sites answer the FIRST request of the day as fast as the hundredth.

        Three defaults in IIS are wrong for this product, and all three show up as the same
        complaint - "the page sometimes takes ages / sometimes does not load":

          startMode = OnDemand + idleTimeout = 20 minutes
              A pool with no traffic for twenty minutes is torn down. The next visitor pays the
              whole cold start. On the panel site that is a second or two of nothing; on the API
              site it is the connection pool, the settings cache and the EF model being rebuilt
              while someone stares at a spinner. A fintech console is used in bursts all day, so
              this fires constantly - a retailer coming back from lunch is always the unlucky one.

          frequentHitThreshold = 2
              Static compression is skipped until a file has been asked for twice within ten
              seconds, so the first visitor after a release downloads the bundle uncompressed
              (~195 KB instead of ~61 KB). Compressing it on the first hit costs nothing - the
              result is cached on disk and served to everyone after.

        Everything here is best-effort and never fails the install: a slower site is not a reason
        to abandon a deployment that otherwise worked.
    #>
    param(
        [string[]] $Folders = @(),
        [string[]] $AppPools = @()
    )

    $appcmd = "$env:SystemRoot\System32\inetsrv\appcmd.exe"
    if (-not (Test-Path $appcmd)) { return }

    # Resolve each folder to the site that serves it, and that site to its application pool.
    $sites = @()
    $pools = [System.Collections.Generic.List[string]]::new()
    foreach ($p in $AppPools) { if ($p) { $pools.Add($p) } }

    try {
        $xml = [xml]((& $appcmd list site /xml 2>$null) -join "`n")
        foreach ($site in $xml.appcmd.SITE) {
            $name = $site.'SITE.NAME'
            $vdirs = & $appcmd list vdir /xml "/app.name:$name/" 2>$null
            if (-not $vdirs) { continue }
            try { $vdirXml = [xml]($vdirs -join "`n") } catch { continue }
            foreach ($vdir in $vdirXml.appcmd.VDIR) {
                $physical = $vdir.physicalPath
                if (-not $physical) { continue }
                foreach ($folder in $Folders) {
                    if (-not $folder) { continue }
                    if ($physical.TrimEnd('\') -ieq $folder.TrimEnd('\')) {
                        $sites += $name
                        $pool = (& $appcmd list app "$name/" /text:applicationPool 2>$null)
                        if ($pool) { $pools.Add(($pool -join '').Trim()) }
                    }
                }
            }
        }
    } catch {
        Write-Note 'Could not read the IIS site list - skipping the performance defaults.'
        return
    }

    foreach ($pool in ($pools | Select-Object -Unique)) {
        if (-not $pool) { continue }
        try {
            & $appcmd set apppool "/apppool.name:$pool" /startMode:AlwaysRunning 2>$null | Out-Null
            & $appcmd set apppool "/apppool.name:$pool" /processModel.idleTimeout:00:00:00 2>$null | Out-Null
            Write-Ok "Application pool '$pool' stays warm (always running, no idle shutdown)"
        } catch {
            Write-Note "Could not set the always-running defaults on pool '$pool'."
        }
    }

    foreach ($site in ($sites | Select-Object -Unique)) {
        try {
            & $appcmd set config "$site/" /section:serverRuntime /frequentHitThreshold:1 /commit:apphost 2>$null | Out-Null
            Write-Ok "Site '$site' compresses static files on the first request"
        } catch {
            Write-Note "Could not lower the compression hit threshold on '$site'."
        }
    }
}

# ---------------------------------------------------------------------------
# Laying one package out into two IIS site folders
# ---------------------------------------------------------------------------
function Copy-PackageToSites {
    <#
        Copies the release into the API site folder and the panel site folder.

        The release carries ONE copy of the build: the API publish, with the operator console
        inside it at wwwroot\panel. That is exactly what the combined (Windows service) install
        runs. For IIS the same files are laid out as two sites - the API without the panel, and the
        panel on its own - which is what this does.

        What it deliberately does not touch, because they belong to the installation rather than to
        the build:
          api\appsettings.Production.json   the connection string, keys and CORS list
          api\wwwroot\uploads               logos, documents, anything operators uploaded
          ui\config.json, ui\app-config.json  the brand, the API address, the phone app's settings
          ui\app\                           the published APK and its download page
        The configurator writes the first and the third of those straight after this returns.

        An update is copied over a running site, where IIS holds the API's DLLs open. The standard
        answer is app_offline.htm: IIS drains the app and releases the files while the rest of the
        server keeps running. It is removed again at the end - including if the copy throws.
    #>
    param(
        [Parameter(Mandatory = $true)] [string] $PackagedApp,
        [Parameter(Mandatory = $true)] [string] $ApiFolder,
        [Parameter(Mandatory = $true)] [string] $UiFolder,
        [Parameter(Mandatory = $true)] [string] $ScriptRoot
    )

    $panelSource = Join-Path $PackagedApp 'wwwroot\panel'
    if (-not (Test-Path (Join-Path $PackagedApp 'FintechApp.dll'))) {
        throw "The package folder does not look like a build: $PackagedApp"
    }
    if (-not (Test-Path $panelSource)) {
        throw "The package has no operator console at $panelSource"
    }

    New-Item -ItemType Directory -Path $ApiFolder, $UiFolder -Force | Out-Null

    $offline = Join-Path $ApiFolder 'app_offline.htm'
    $liveSite = Test-Path (Join-Path $ApiFolder 'FintechApp.dll')
    if ($liveSite) {
        Write-Note 'The API site is already running here - taking it offline for the copy.'
        [System.IO.File]::WriteAllText($offline,
            '<html><body style="font-family:Segoe UI,Arial;padding:3rem"><h2>Updating</h2><p>This service is being updated and will be back in a moment.</p></body></html>')
        # IIS notices the file and unloads the application; the DLLs are released a moment later.
        Start-Sleep -Seconds 3
    }

    try {
        Write-Note "Copying the API into $ApiFolder"
        $apiLog = Invoke-RobocopyTree -Source $PackagedApp -Destination $ApiFolder `
            -ExcludeDirs @((Join-Path $PackagedApp 'wwwroot\panel'), (Join-Path $PackagedApp 'wwwroot\uploads')) `
            -ExcludeFiles @('appsettings.Production.json', 'app_offline.htm')
        if ($apiLog) { Write-Warn2 $apiLog }

        # The panel's own branded files are the installation's, not the build's. When this run is
        # not applying a new logo they must survive the copy, so they are put back afterwards.
        $keep = @{}
        foreach ($name in @('favicon.ico', 'favicon.svg')) {
            $path = Join-Path $UiFolder $name
            if (Test-Path $path) { $keep[$name] = [System.IO.File]::ReadAllBytes($path) }
        }

        Write-Note "Copying the operator console into $UiFolder"
        $uiLog = Invoke-RobocopyTree -Source $panelSource -Destination $UiFolder `
            -ExcludeFiles @('config.json', 'app-config.json')
        if ($uiLog) { Write-Warn2 $uiLog }

        foreach ($name in $keep.Keys) {
            [System.IO.File]::WriteAllBytes((Join-Path $UiFolder $name), $keep[$name])
        }

        # The panel is a single-page app: IIS has to serve index.html for its routes, must not do
        # that for a missing file with an extension, and has to know what a .apk is.
        $uiWebConfig = Join-Path $ScriptRoot 'iis\ui.web.config'
        if (Test-Path $uiWebConfig) {
            $target = Join-Path $UiFolder 'web.config'
            if (Test-Path $target) { Copy-Item $target "$target.backup" -Force }
            Copy-Item $uiWebConfig $target -Force
        }

        Write-Ok 'Files in place (api + panel)'
    } finally {
        if (Test-Path $offline) { Remove-Item $offline -Force -ErrorAction SilentlyContinue }
    }
}

function Invoke-RobocopyTree {
    <#
        robocopy, with its output captured and its exit code translated.

        robocopy returns a bit field: 0-7 mean "copied / nothing to do / extra files present", and
        8 and above are real failures. PowerShell would otherwise treat any output on stderr as a
        NativeCommandError, so it runs through cmd with both streams redirected to a log.

        Returns $null on success, or the tail of the log when robocopy actually failed.
    #>
    param(
        [Parameter(Mandatory = $true)] [string] $Source,
        [Parameter(Mandatory = $true)] [string] $Destination,
        [string[]] $ExcludeDirs = @(),
        [string[]] $ExcludeFiles = @()
    )

    $log = Join-Path $env:TEMP ("fino-copy-{0}.log" -f ([guid]::NewGuid().ToString('N')))
    $arguments = @("`"$($Source.TrimEnd('\'))`"", "`"$($Destination.TrimEnd('\'))`"", '/E', '/R:2', '/W:2', '/NFL', '/NDL', '/NP', '/NJH')
    if ($ExcludeDirs.Count  -gt 0) { $arguments += '/XD'; $arguments += ($ExcludeDirs  | ForEach-Object { "`"$_`"" }) }
    if ($ExcludeFiles.Count -gt 0) { $arguments += '/XF'; $arguments += ($ExcludeFiles | ForEach-Object { "`"$_`"" }) }

    cmd /c "robocopy $($arguments -join ' ') > `"$log`" 2>&1"
    $code = $LASTEXITCODE

    $tail = if (Test-Path $log) { (Get-Content $log -Tail 12) -join "`n" } else { '' }
    Remove-Item $log -Force -ErrorAction SilentlyContinue

    if ($code -ge 8) { throw "Copying $Source to $Destination failed (robocopy $code):`n$tail" }

    # robocopy's "success" codes are 1-7, and PowerShell keeps the last native exit code around.
    # A caller that ends with `exit $LASTEXITCODE` would then report a failure for a copy that
    # worked, so it is cleared here rather than in every caller.
    $global:LASTEXITCODE = 0
    return $null
}

# ---------------------------------------------------------------------------
# Service control that actually waits
# ---------------------------------------------------------------------------
function Stop-FinoService {
    param([string] $ServiceName, [int] $TimeoutSeconds = 90)
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service -or $service.Status -eq 'Stopped') { return $true }

    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    $waited = 0
    while ((Get-Service -Name $ServiceName).Status -ne 'Stopped' -and $waited -lt $TimeoutSeconds) {
        Start-Sleep -Seconds 1; $waited++
    }
    # The listening socket can outlive the process by a moment; the next start would then fail with
    # "address already in use".
    Start-Sleep -Seconds 2
    return (Get-Service -Name $ServiceName).Status -eq 'Stopped'
}

function Start-FinoService {
    param([string] $ServiceName, [int] $Attempts = 3)
    foreach ($attempt in 1..$Attempts) {
        try {
            Start-Service -Name $ServiceName -ErrorAction Stop
            return $true
        } catch {
            if ($attempt -lt $Attempts) { Start-Sleep -Seconds 5 }
        }
    }
    return $false
}

<#
    Polls /health until the application says it is ready.

    A brand-new installation answers 503 "starting" for two to three minutes while it builds its
    database, which Invoke-RestMethod raises as an error - so the body is read out of the exception
    instead of being thrown away. A setup that has genuinely failed says so there, and that message
    is the entire diagnosis for whoever is doing the install.
#>
function Wait-FinoHealthy {
    param(
        [string] $HealthUrl,
        [int]    $TimeoutMinutes = 12,
        [string] $ServiceName,
        [switch] $Quiet
    )

    $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
    $spinner  = '|/-\'
    $i = 0

    while ((Get-Date) -lt $deadline) {
        if ($ServiceName) {
            $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
            if ($service -and $service.Status -eq 'Stopped') {
                if (-not $Quiet) { Write-Host '' }
                return @{ ok = $false; reason = 'service-stopped' }
            }
        }

        try {
            $response = Invoke-RestMethod -Uri $HealthUrl -TimeoutSec 5 -ErrorAction Stop
            if ($response.status -eq 'ok') {
                if (-not $Quiet) { Write-Host "`r                    " -NoNewline; Write-Host "`r" -NoNewline }
                return @{ ok = $true }
            }
        } catch {
            $body = $null
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                $body = (New-Object System.IO.StreamReader($stream)).ReadToEnd() | ConvertFrom-Json
            } catch { }

            if ($body -and $body.status -eq 'setup-failed') {
                if (-not $Quiet) { Write-Host '' }
                return @{ ok = $false; reason = 'setup-failed'; error = $body.error }
            }
        }

        if (-not $Quiet) { Write-Host ("`r  working {0}  " -f $spinner[$i++ % 4]) -NoNewline }
        Start-Sleep -Seconds 5
    }

    if (-not $Quiet) { Write-Host '' }
    return @{ ok = $false; reason = 'timeout' }
}
