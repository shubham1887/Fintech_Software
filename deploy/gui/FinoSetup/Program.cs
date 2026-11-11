using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FinoSetup;

internal static class Program
{
    // A WinExe has no console of its own. For the two diagnostic switches below, borrow the
    // console of whatever started it, so the output is actually readable.
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int processId);
    private const int AttachParentProcess = -1;

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0) AttachConsole(AttachParentProcess);

        ApplicationConfiguration.Initialize();
        Application.SetCompatibleTextRenderingDefault(false);

        // Development aid: render each screen to a PNG and exit, so the layout can be reviewed
        // without a desktop session. Not reachable by double-clicking.
        if (args.Length >= 2 && args[0] == "--screenshot")
        {
            return Screenshot.WriteAll(args[1]);
        }

        // Checks the seam that matters - this exe calling the PowerShell scripts and reading back
        // what they say about the server - without installing anything. Run it from a release
        // folder on a real server when something looks wrong.
        if (args.Length >= 1 && args[0] == "--selftest")
        {
            return SelfTest.Run();
        }

        Application.EnableVisualStyles();
        Application.Run(new MainForm());
        return 0;
    }
}

/// <summary>
/// Runs the same probe the first screen runs, and prints what came back. Everything the graphical
/// setup does goes through PowerShell, so if this works the plumbing works.
/// </summary>
internal static class SelfTest
{
    public static int Run()
    {
        Console.WriteLine($"release folder : {ScriptRunner.ReleaseFolder}");
        Console.WriteLine($"Setup.ps1 found: {ScriptRunner.HasScripts}");

        if (!ScriptRunner.HasScripts)
        {
            Console.WriteLine("FAIL: this is not an unzipped release folder.");
            return 1;
        }

        try
        {
            var probe = ScriptRunner.ProbeAsync().GetAwaiter().GetResult();
            Console.WriteLine($"release        : {probe.ReleaseVersion}");
            Console.WriteLine($"application    : {(probe.HasApplication ? "present" : "MISSING")}");
            Console.WriteLine($"elevated       : {probe.IsAdmin}");
            Console.WriteLine($"os             : {probe.Os}");
            Console.WriteLine($"memory / disk  : {probe.RamGb} GB / {probe.FreeDiskGb} GB free");
            Console.WriteLine($"sql instances  : {(probe.SqlInstances.Count == 0 ? "none" : string.Join(", ", probe.SqlInstances.Select(i => $"{i.ServerName} ({i.Status})")))}");
            Console.WriteLine(probe.Installed is null
                ? "installed      : nothing on this server"
                : $"installed      : {probe.Installed.Mode} {probe.Installed.BrandName} {probe.Installed.Version} at {probe.Installed.InstallPath}");
            Console.WriteLine("OK");
            return 0;
        }
        catch (Exception exception)
        {
            Console.WriteLine("FAIL: " + exception.Message);
            return 1;
        }
    }
}

/// <summary>
/// Renders the screens off-screen. DrawToBitmap works on a form that has been laid out but never
/// shown, which is what makes this usable over a remote session with no interactive desktop.
/// </summary>
internal static class Screenshot
{
    public static int WriteAll(string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);

        var samples = new (string Name, ProbeResult Probe, string Screen)[]
        {
            ("home-fresh", FreshServer(), "home"),
            ("home-installed", InstalledServer(), "home"),
            ("home-legacy", LegacyIisServer(), "home"),
            ("install-form", FreshServer(), "install"),
            ("running", InstalledServer(), "running"),
            ("done", InstalledServer(), "done"),
            ("failed", InstalledServer(), "failed")
        };

        foreach (var sample in samples)
        {
            using var form = new MainForm { ScreenshotMode = true };

            // Shown off-screen rather than never shown: Windows Forms only paints child controls
            // once the window handle is created and the window has been through a paint cycle, so
            // DrawToBitmap on a form that was never shown returns an empty frame.
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-6000, -6000);
            form.ShowInTaskbar = false;
            form.Show();
            Application.DoEvents();

            form.RenderForScreenshot(sample.Probe, sample.Screen);
            Application.DoEvents();
            Thread.Sleep(250);
            Application.DoEvents();

            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(0, 0, form.Width, form.Height));
            bitmap.Save(Path.Combine(outputFolder, sample.Name + ".png"), ImageFormat.Png);
            Console.WriteLine($"wrote {sample.Name}.png");

            form.Close();
        }

        return 0;
    }

    private static ProbeResult FreshServer() => new()
    {
        ReleaseVersion = "2026.09.18.1",
        HasApplication = true,
        IsAdmin = true,
        Os = "Microsoft Windows Server 2022 Standard",
        RamGb = 8,
        FreeDiskGb = 120.4,
        Is64Bit = true,
        SqlInstances = [new SqlInstance { Name = "SQLEXPRESS", ServerName = ".\\SQLEXPRESS", Status = "Running" }],
        Installed = null
    };

    private static ProbeResult InstalledServer() => new()
    {
        ReleaseVersion = "2026.09.18.1",
        HasApplication = true,
        IsAdmin = true,
        Os = "Microsoft Windows Server 2019 Standard",
        RamGb = 16,
        FreeDiskGb = 64.2,
        Is64Bit = true,
        SqlInstances = [new SqlInstance { Name = "SQLEXPRESS", ServerName = ".\\SQLEXPRESS", Status = "Running" }],
        VersionCompare = 1,
        Installed = new InstalledInfo
        {
            Mode = "service",
            InstallPath = @"C:\Fino",
            ServiceName = "FinoAPI",
            Version = "2026.09.02.3",
            BrandName = "RiPay",
            Port = 8080,
            Scheme = "http",
            PublicUrl = "https://panel.ripay.co.in",
            Database = "RiPay",
            SqlServer = ".\\SQLEXPRESS"
        }
    };

    private static ProbeResult LegacyIisServer() => new()
    {
        ReleaseVersion = "2026.09.18.1",
        HasApplication = true,
        IsAdmin = true,
        Os = "Microsoft Windows Server 2019 Standard",
        RamGb = 16,
        FreeDiskGb = 48.9,
        Is64Bit = true,
        SqlInstances = [new SqlInstance { Name = "SQLEXPRESS", ServerName = ".\\SQLEXPRESS", Status = "Running" }],
        VersionCompare = -1,
        Installed = new InstalledInfo
        {
            Mode = "iis",
            InstallPath = @"C:\inetpub\wwwroot\api.client.co.in",
            SiteName = "api.client.co.in",
            Version = "unknown"
        }
    };
}
