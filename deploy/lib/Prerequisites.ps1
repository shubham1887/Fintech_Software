<#
    Server prerequisites for a Fino installation.

    The short version of what a new server needs: SQL Server. That is all.

    Not needed, deliberately, because each one used to be a manual step that could be got wrong:
      * IIS            - the application hosts itself as a Windows service (Kestrel).
      * .NET runtime   - the release is published self-contained; the runtime is inside it.
      * Node.js        - the operator console is built before shipping; the server only serves files.
      * URL Rewrite / ASP.NET Core Hosting Bundle - both are IIS pieces, and there is no IIS.

    Dot-source after FinoCommon.ps1.
#>

# ---------------------------------------------------------------------------
# What is already here
# ---------------------------------------------------------------------------
function Get-SqlInstances {
    <#
        Reads the instance names SQL Server registers for itself. More reliable than looking for
        services by name, because a named instance's service is MSSQL$<NAME> and a default instance's
        is MSSQLSERVER.
    #>
    $key = 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL'
    if (-not (Test-Path $key)) { return @() }

    $instances = @()
    $properties = Get-ItemProperty $key
    foreach ($property in $properties.PSObject.Properties) {
        if ($property.Name -in @('PSPath', 'PSParentPath', 'PSChildName', 'PSDrive', 'PSProvider')) { continue }

        $name = $property.Name
        $serviceName = if ($name -eq 'MSSQLSERVER') { 'MSSQLSERVER' } else { "MSSQL`$$name" }
        $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

        $instances += [pscustomobject]@{
            Name        = $name
            # What goes in a connection string: a default instance is just the machine.
            ServerName  = if ($name -eq 'MSSQLSERVER') { 'localhost' } else { ".\$name" }
            ServiceName = $serviceName
            Status      = if ($service) { $service.Status.ToString() } else { 'Not installed' }
            InstanceId  = $property.Value
        }
    }
    return $instances
}

function Get-ServerFacts {
    $os   = Get-CimInstance Win32_OperatingSystem
    $disk = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='$($env:SystemDrive)'"

    [pscustomobject]@{
        OsName        = $os.Caption
        OsVersion     = [Version]$os.Version
        RamGb         = [math]::Round($os.TotalVisibleMemorySize / 1MB, 1)
        FreeDiskGb    = [math]::Round($disk.FreeSpace / 1GB, 1)
        Is64Bit       = [Environment]::Is64BitOperatingSystem
        SqlInstances  = @(Get-SqlInstances)
    }
}

function Show-ServerCheck {
    <#
        Prints what was found and returns $true when the server can host an installation at all.
        Soft limits (RAM, disk) warn; hard ones (32-bit, ancient Windows) stop.
    #>
    param([object] $Facts)

    Write-Step 'Checking the server'

    Write-Note "$($Facts.OsName)"
    $ok = $true

    if (-not $Facts.Is64Bit) {
        Write-Fail '32-bit Windows. SQL Server and this application both need 64-bit.'
        $ok = $false
    }

    # Windows Server 2012 R2 / Windows 8.1 is 6.3. Anything older is out of support and will not
    # have a usable TLS stack for the payment providers this platform talks to.
    if ($Facts.OsVersion -lt [Version]'6.3') {
        Write-Fail "Windows version $($Facts.OsVersion) is too old. Windows Server 2016 or newer is expected."
        $ok = $false
    }

    if ($Facts.RamGb -lt 3.5) {
        Write-Warn2 "$($Facts.RamGb) GB RAM. SQL Server alone wants 2 GB - 4 GB total is the realistic minimum."
    } else {
        Write-Ok "$($Facts.RamGb) GB RAM"
    }

    if ($Facts.FreeDiskGb -lt 15) {
        Write-Warn2 "$($Facts.FreeDiskGb) GB free on $($env:SystemDrive). SQL Server plus the application needs about 15 GB to install comfortably."
    } else {
        Write-Ok "$($Facts.FreeDiskGb) GB free on $($env:SystemDrive)"
    }

    if ($Facts.SqlInstances.Count -gt 0) {
        foreach ($instance in $Facts.SqlInstances) {
            Write-Ok "SQL Server found: $($instance.ServerName)  ($($instance.Status))"
        }
    } else {
        Write-Note 'SQL Server: not installed (the setup can install SQL Server Express for you)'
    }

    Write-Note 'IIS, .NET and Node.js are NOT required - this build hosts itself and ships its own runtime.'

    return $ok
}

# ---------------------------------------------------------------------------
# Installing SQL Server Express
# ---------------------------------------------------------------------------

# Microsoft's permanent short link for the current SQL Server Express bootstrapper. The
# bootstrapper is small; it downloads the real installation media itself, which is why this is two
# steps rather than one.
$script:SqlExpressBootstrapperUrl = 'https://go.microsoft.com/fwlink/p/?linkid=2216019'
$script:SqlExpressFallbackUrl     = 'https://go.microsoft.com/fwlink/?linkid=866658'   # SQL Server 2019 Express

function Install-SqlServerExpress {
    <#
        Downloads and silently installs SQL Server Express with a named instance.

        Choices made here, and why:
          * Windows authentication only. The application's service account is added as a sysadmin,
            so there is no "sa" password to store in a config file, print in a summary, or leak.
          * SQLEngine only. No SSRS, no full-text, no analysis services - none of it is used, and
            every extra feature is more attack surface and more to patch.
          * TCP/IP and SQL Browser enabled, so the panel's own tooling and any later remote
            connection work without another manual trip to Configuration Manager.

        Returns the connection-string server name (".\SQLEXPRESS"), or $null on failure.
    #>
    param(
        [string] $InstanceName = 'SQLEXPRESS',
        [string] $WorkFolder = (Join-Path $env:TEMP 'fino-sql-setup'),
        # Accounts to make sysadmin. The service runs as LocalSystem by default, so that is here.
        [string[]] $SysAdminAccounts = @('BUILTIN\Administrators', 'NT AUTHORITY\SYSTEM')
    )

    Write-Step "Installing SQL Server Express (instance $InstanceName)"
    Write-Host '  This downloads about 300 MB from Microsoft and takes 10-20 minutes.' -ForegroundColor DarkGray
    Write-Host '  Leave this window open.' -ForegroundColor DarkGray

    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    New-Item -ItemType Directory -Path $WorkFolder -Force | Out-Null

    # --- 1. the bootstrapper -------------------------------------------------
    $bootstrapper = Join-Path $WorkFolder 'SQL-Express-Setup.exe'
    if (-not (Test-Path $bootstrapper)) {
        Write-Host '  Downloading the SQL Server Express installer...'
        $downloaded = $false
        foreach ($url in @($script:SqlExpressBootstrapperUrl, $script:SqlExpressFallbackUrl)) {
            try {
                # Invoke-WebRequest's progress bar makes a large download several times slower in
                # Windows PowerShell; this is the documented way around it.
                $previous = $ProgressPreference
                $ProgressPreference = 'SilentlyContinue'
                Invoke-WebRequest -Uri $url -OutFile $bootstrapper -UseBasicParsing -TimeoutSec 600
                $ProgressPreference = $previous
                $downloaded = $true
                break
            } catch {
                Write-Warn2 "Download failed from $url - $($_.Exception.Message)"
            }
        }

        if (-not $downloaded) {
            Write-Fail 'Could not download SQL Server Express.'
            Write-Host '  This server may have no internet access, or a firewall is blocking it.' -ForegroundColor Yellow
            Write-Host '  Install SQL Server Express by hand from another machine, then run this setup again:' -ForegroundColor Yellow
            Write-Host '    https://www.microsoft.com/sql-server/sql-server-downloads' -ForegroundColor DarkGray
            return $null
        }
    }
    Write-Ok 'Installer downloaded'

    # --- 2. the real media ---------------------------------------------------
    $mediaFolder = Join-Path $WorkFolder 'media'
    New-Item -ItemType Directory -Path $mediaFolder -Force | Out-Null

    $media = Get-ChildItem $mediaFolder -Filter 'SQLEXPR*.exe' -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $media) {
        Write-Host '  Fetching the installation files (this is the big download)...'
        $process = Start-Process -FilePath $bootstrapper -Wait -PassThru -ArgumentList @(
            '/Action=Download', '/MediaPath="' + $mediaFolder + '"', '/MediaType=Core', '/Quiet', '/HideProgressBar'
        )
        if ($process.ExitCode -ne 0) {
            Write-Fail "The download step failed (exit code $($process.ExitCode))."
            return $null
        }
        $media = Get-ChildItem $mediaFolder -Filter 'SQLEXPR*.exe' -ErrorAction SilentlyContinue | Select-Object -First 1
    }
    if (-not $media) { Write-Fail 'The installation files did not arrive.'; return $null }
    Write-Ok "Installation files ready ($([math]::Round($media.Length / 1MB)) MB)"

    # --- 3. extract ----------------------------------------------------------
    $extracted = Join-Path $WorkFolder 'extracted'
    if (-not (Test-Path (Join-Path $extracted 'setup.exe'))) {
        Write-Host '  Unpacking...'
        $process = Start-Process -FilePath $media.FullName -Wait -PassThru -ArgumentList @('/q', "/x:`"$extracted`"")
        if ($process.ExitCode -ne 0) { Write-Fail "Unpacking failed (exit code $($process.ExitCode))."; return $null }
    }
    if (-not (Test-Path (Join-Path $extracted 'setup.exe'))) { Write-Fail 'Unpacking produced no setup.exe.'; return $null }
    Write-Ok 'Unpacked'

    # --- 4. install ----------------------------------------------------------
    Write-Host '  Installing SQL Server (10-15 minutes)...'
    $sysAdmins = ($SysAdminAccounts | ForEach-Object { "`"$_`"" }) -join ' '
    $arguments = @(
        '/Q',
        '/IACCEPTSQLSERVERLICENSETERMS',
        '/ACTION=Install',
        '/FEATURES=SQLEngine',
        "/INSTANCENAME=$InstanceName",
        '/SQLSVCACCOUNT="NT AUTHORITY\NETWORK SERVICE"',
        "/SQLSYSADMINACCOUNTS=$sysAdmins",
        '/TCPENABLED=1',
        '/NPENABLED=1',
        '/BROWSERSVCSTARTUPTYPE=Automatic',
        '/SQLSVCSTARTUPTYPE=Automatic',
        '/UPDATEENABLED=False'
    )

    $process = Start-Process -FilePath (Join-Path $extracted 'setup.exe') -Wait -PassThru -ArgumentList $arguments

    # 3010 means "installed, reboot wanted". The database engine is usable before that reboot.
    if ($process.ExitCode -notin @(0, 3010)) {
        Write-Fail "SQL Server setup failed (exit code $($process.ExitCode))."
        Write-Host '  The detailed log is under:' -ForegroundColor Yellow
        Write-Host '    C:\Program Files\Microsoft SQL Server\<version>\Setup Bootstrap\Log\Summary.txt' -ForegroundColor DarkGray
        return $null
    }
    if ($process.ExitCode -eq 3010) { Write-Warn2 'SQL Server asks for a reboot at some point - it works until then.' }

    Write-Ok "SQL Server Express installed as $InstanceName"

    Enable-SqlNetworking -InstanceName $InstanceName

    $serverName = ".\$InstanceName"
    $test = Test-SqlConnection "Server=$serverName;Database=master;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
    if (-not $test.ok) {
        Write-Fail "SQL Server was installed but cannot be reached: $($test.error)"
        return $null
    }
    Write-Ok "Connected: $($test.version)"

    return $serverName
}

function Enable-SqlNetworking {
    <#
        Turns on TCP/IP for the instance and starts the SQL Browser service.

        Why bother when the application connects locally: without the Browser, a named instance can
        only be resolved by name from the same machine through shared memory. The moment anything
        addresses it as MACHINE\SQLEXPRESS - a backup tool, a reporting connection, a second server
        later - it fails with the famously unhelpful "error 26". Turning both on now costs nothing
        and removes a whole category of support call.
    #>
    param([string] $InstanceName = 'SQLEXPRESS')

    $serviceName = if ($InstanceName -eq 'MSSQLSERVER') { 'MSSQLSERVER' } else { "MSSQL`$$InstanceName" }

    # TCP/IP lives in the registry under the instance id; the id is not the instance name.
    $instanceKey = 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL'
    $changed = $false
    if (Test-Path $instanceKey) {
        $instanceId = (Get-ItemProperty $instanceKey).$InstanceName
        if ($instanceId) {
            $tcpKey = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$instanceId\MSSQLServer\SuperSocketNetLib\Tcp"
            if (Test-Path $tcpKey) {
                $current = (Get-ItemProperty $tcpKey).Enabled
                if ($current -ne 1) {
                    Set-ItemProperty -Path $tcpKey -Name 'Enabled' -Value 1
                    $changed = $true
                }
            }
        }
    }

    try {
        $browser = Get-Service -Name 'SQLBrowser' -ErrorAction Stop
        if ($browser.StartType -eq 'Disabled') { Set-Service -Name 'SQLBrowser' -StartupType Automatic }
        if ($browser.Status -ne 'Running') { Start-Service -Name 'SQLBrowser' -ErrorAction SilentlyContinue }
        Write-Ok 'SQL Browser running (named instances resolve by name)'
    } catch {
        Write-Note 'SQL Browser service is not present on this machine.'
    }

    if ($changed) {
        Write-Host '  Restarting SQL Server so TCP/IP takes effect...'
        Restart-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        Write-Ok 'TCP/IP enabled'
    }
}

function Resolve-SqlServerForInstall {
    <#
        Works out which SQL Server the installation should use, installing one if the server has
        none. Returns the connection-string server name, or $null if the person running setup
        chose to stop.
    #>
    param([object] $Facts, [switch] $Unattended, [string] $PreferredServer)

    if ($PreferredServer) { return $PreferredServer }

    $running = @($Facts.SqlInstances | Where-Object { $_.Status -eq 'Running' })
    $stopped = @($Facts.SqlInstances | Where-Object { $_.Status -ne 'Running' })

    # A stopped instance is almost always one somebody installed and never started; starting it is
    # better than installing a second copy beside it.
    foreach ($instance in $stopped) {
        Write-Note "SQL Server instance $($instance.ServerName) is $($instance.Status) - starting it"
        try {
            Set-Service -Name $instance.ServiceName -StartupType Automatic -ErrorAction SilentlyContinue
            Start-Service -Name $instance.ServiceName -ErrorAction Stop
            $running += $instance
        } catch {
            Write-Warn2 "Could not start $($instance.ServiceName): $($_.Exception.Message)"
        }
    }

    if ($running.Count -eq 1) {
        Write-Ok "Using SQL Server at $($running[0].ServerName)"
        Enable-SqlNetworking -InstanceName $running[0].Name
        return $running[0].ServerName
    }

    if ($running.Count -gt 1) {
        if ($Unattended) { return $running[0].ServerName }
        Write-Host ''
        Write-Host '  This server has more than one SQL Server:' -ForegroundColor White
        for ($i = 0; $i -lt $running.Count; $i++) {
            Write-Host ("    {0}. {1}" -f ($i + 1), $running[$i].ServerName)
        }
        $choice = Ask '  Which one should Fino use? (number)' -Default '1'
        $index = [int]$choice - 1
        if ($index -lt 0 -or $index -ge $running.Count) { $index = 0 }
        Enable-SqlNetworking -InstanceName $running[$index].Name
        return $running[$index].ServerName
    }

    # Nothing installed.
    if (-not $Unattended) {
        Write-Host ''
        Write-Host '  This server has no database engine yet.' -ForegroundColor White
        Write-Host '  Fino needs Microsoft SQL Server. The free Express edition is enough to start with' -ForegroundColor DarkGray
        Write-Host '  (it holds up to 10 GB of data).' -ForegroundColor DarkGray
        if (-not (Ask-YesNo '  Download and install SQL Server Express now?' $true)) {
            Write-Fail 'Cannot continue without a database.'
            return $null
        }
    }

    return Install-SqlServerExpress
}
