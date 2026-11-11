using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace FinoSetup;

/// <summary>
/// Runs the deployment kit's PowerShell scripts and streams their output back.
///
/// The graphical setup deliberately owns none of the installation logic. Everything it does ends up
/// here, calling the same Setup.ps1 / Set-Branding.ps1 / Backup-Fino.ps1 a command-line user runs,
/// so there is exactly one implementation to keep correct and to fix when a server surprises us.
/// </summary>
public static class ScriptRunner
{
    /// <summary>The folder the release was unzipped into - where Setup.ps1 and app\ live.</summary>
    public static string ReleaseFolder { get; } = AppContext.BaseDirectory.TrimEnd('\\');

    public static string ScriptPath(string name) => Path.Combine(ReleaseFolder, name);

    public static bool HasScripts => File.Exists(ScriptPath("Setup.ps1"));

    /// <summary>
    /// Asks Setup.ps1 what this server looks like. Runs quickly and changes nothing.
    /// </summary>
    public static async Task<ProbeResult> ProbeAsync(CancellationToken cancellation = default)
    {
        var output = new StringBuilder();
        await RunAsync(
            arguments: BuildArguments("Setup.ps1", "-Probe"),
            onOutput: line => output.AppendLine(line),
            cancellation: cancellation);

        var text = output.ToString();
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new InvalidOperationException(
                "The server check did not return anything usable." + Environment.NewLine + text.Trim());
        }

        return ProbeResult.Parse(text.Substring(start, end - start + 1));
    }

    /// <summary>
    /// Runs an installation, update or migration from collected answers.
    /// </summary>
    public static async Task<int> RunSetupAsync(
        InstallAnswers answers,
        Action<string> onOutput,
        CancellationToken cancellation = default)
    {
        var answerFile = WriteProtectedAnswerFile(answers);
        try
        {
            // The migration warnings were shown and accepted in the dialog before we got here; the
            // script refuses to switch a live deployment over without that confirmation.
            var arguments = $"-AnswerFile \"{answerFile}\"";
            if (string.Equals(answers.Mode, "migrate", StringComparison.OrdinalIgnoreCase))
            {
                arguments += " -ConfirmMigration";
            }

            return await RunAsync(BuildArguments("Setup.ps1", arguments), onOutput, cancellation);
        }
        finally
        {
            TryDelete(answerFile);
        }
    }

    public static Task<int> RunBrandingAsync(
        string installPath, string brandName, string? logoPath,
        Action<string> onOutput, CancellationToken cancellation = default)
    {
        var arguments = $"-InstallPath \"{installPath}\" -BrandName \"{brandName.Replace("\"", "`\"")}\"";
        if (!string.IsNullOrWhiteSpace(logoPath)) arguments += $" -LogoPath \"{logoPath}\"";
        return RunAsync(BuildArguments("Set-Branding.ps1", arguments), onOutput, cancellation);
    }

    public static Task<int> RunBackupAsync(
        string installPath, Action<string> onOutput, CancellationToken cancellation = default)
    {
        return RunAsync(
            BuildArguments("Backup-Fino.ps1", $"-InstallPath \"{installPath}\""),
            onOutput, cancellation);
    }

    // -----------------------------------------------------------------------
    // Plumbing
    // -----------------------------------------------------------------------
    private static string BuildArguments(string scriptName, string scriptArguments)
    {
        var script = ScriptPath(scriptName);
        // -ExecutionPolicy Bypass applies to this one process; no machine policy is changed.
        return $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{script}\" {scriptArguments}";
    }

    private static async Task<int> RunAsync(
        string arguments, Action<string> onOutput, CancellationToken cancellation)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = arguments,
            WorkingDirectory = ReleaseFolder,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        var completion = new TaskCompletionSource<int>();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) onOutput(e.Data); };
        // stderr is shown in the same stream: PowerShell writes warnings there, and hiding them
        // from someone trying to work out why an install failed helps nobody.
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) onOutput(e.Data); };
        process.Exited += (_, _) => completion.TrySetResult(process.ExitCode);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using (cancellation.Register(() =>
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { /* already gone */ }
        }))
        {
            return await completion.Task;
        }
    }

    /// <summary>
    /// Writes the answers to a temporary file only Administrators and SYSTEM can read.
    /// It holds the first administrator's password until the run finishes.
    /// </summary>
    private static string WriteProtectedAnswerFile(InstallAnswers answers)
    {
        var path = Path.Combine(Path.GetTempPath(), $"fino-setup-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, answers.ToJson(), new UTF8Encoding(false));

        try
        {
            var file = new FileInfo(path);
            var security = file.GetAccessControl();
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);

            foreach (var account in new[]
                     {
                         new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                         new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null)
                     })
            {
                security.AddAccessRule(new FileSystemAccessRule(
                    account, FileSystemRights.FullControl, AccessControlType.Allow));
            }

            file.SetAccessControl(security);
        }
        catch
        {
            // A locked-down ACL is a safeguard, not the safeguard: the file is short-lived and in a
            // folder only administrators can reach anyway. Failing the install over it would be worse.
        }

        return path;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            // The password should not survive in free space any longer than it has to.
            var length = (int)new FileInfo(path).Length;
            File.WriteAllBytes(path, new byte[length]);
            File.Delete(path);
        }
        catch { /* the file is in TEMP; Windows will get to it */ }
    }
}
