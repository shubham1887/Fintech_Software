using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinoSetup;

/// <summary>
/// What `Setup.ps1 -Probe` reports about this server. Everything the first screen shows comes from
/// here, so the graphical setup and the command line always agree about what they are looking at.
/// </summary>
public sealed class ProbeResult
{
    public string ReleaseVersion { get; set; } = "unknown";
    public bool HasApplication { get; set; }
    public bool IsAdmin { get; set; }
    public string Os { get; set; } = "";
    public double RamGb { get; set; }
    public double FreeDiskGb { get; set; }
    public bool Is64Bit { get; set; }
    public List<SqlInstance> SqlInstances { get; set; } = new();
    public InstalledInfo? Installed { get; set; }

    /// <summary>1 = this release is newer, 0 = same, -1 = older, null = nothing installed.</summary>
    public int? VersionCompare { get; set; }

    public bool HasRunningSql => SqlInstances.Any(i => string.Equals(i.Status, "Running", StringComparison.OrdinalIgnoreCase));

    public static ProbeResult Parse(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        return JsonSerializer.Deserialize<ProbeResult>(json, options) ?? new ProbeResult();
    }
}

public sealed class SqlInstance
{
    public string Name { get; set; } = "";
    public string ServerName { get; set; } = "";
    public string Status { get; set; } = "";
}

public sealed class InstalledInfo
{
    /// <summary>"service" for an installation from this kit, "iis" for an older hand-made one.</summary>
    public string Mode { get; set; } = "";
    public string? InstallPath { get; set; }
    public string? ServiceName { get; set; }
    public string? SiteName { get; set; }
    public string? Version { get; set; }
    public string? BrandName { get; set; }
    public int? Port { get; set; }
    public string? Scheme { get; set; }
    public string? PublicUrl { get; set; }
    public string? Database { get; set; }
    public string? SqlServer { get; set; }

    /// <summary>Running / Stopped / not installed - and Automatic, Manual or Disabled.</summary>
    public string? ServiceState { get; set; }
    public string? ServiceStart { get; set; }

    /// <summary>
    /// A stopped or disabled service on a box that also has IIS sites usually means this
    /// installation is dormant and something else is serving the same database. Updating it would
    /// bring up a second application against that database.
    /// </summary>
    public bool LooksDormant =>
        !IsLegacyIis &&
        (string.Equals(ServiceStart, "Disabled", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(ServiceState, "Stopped", StringComparison.OrdinalIgnoreCase));

    public bool IsLegacyIis => string.Equals(Mode, "iis", StringComparison.OrdinalIgnoreCase);

    public string LocalUrl =>
        $"{(string.IsNullOrWhiteSpace(Scheme) ? "http" : Scheme)}://localhost:{(Port is > 0 ? Port : 8080)}";
}

/// <summary>
/// The answers collected on the form, handed to Setup.ps1 as a file.
///
/// A file rather than a command line because of the password: command lines are visible to anyone
/// who can list processes on the machine. The file is written with an ACL limited to Administrators
/// and SYSTEM, and deleted as soon as the run finishes.
/// </summary>
public sealed class InstallAnswers
{
    public string Mode { get; set; } = "install";
    public string BrandName { get; set; } = "";
    public string? LogoPath { get; set; }
    public string AdminName { get; set; } = "Administrator";
    public string AdminMobile { get; set; } = "";
    public string AdminEmail { get; set; } = "";
    public string AdminPassword { get; set; } = "";
    public string PublicUrl { get; set; } = "";
    public int Port { get; set; } = 8080;
    public string InstallPath { get; set; } = @"C:\Fino";
    public string DatabaseName { get; set; } = "FinoApp";
    public string SqlServer { get; set; } = "";
    public string ServiceName { get; set; } = "FinoAPI";

    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}
