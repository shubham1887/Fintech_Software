using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FinoSetup;

/// <summary>
/// The whole window. Screens are plain panels swapped into the content area and rebuilt each time
/// they are shown - a setup wizard is short-lived and linear, so keeping state in the controls
/// rather than in a model layer is simpler to follow and harder to get wrong.
/// </summary>
public sealed class MainForm : Form
{
    private readonly Panel _content;
    private readonly Label _headerSubtitle;
    private ProbeResult? _probe;
    private CancellationTokenSource? _running;

    public MainForm()
    {
        Text = "Fino Setup";
        // Tall enough that the install form fits without scrolling, short enough for a 768-pixel
        // server console.
        Size = new Size(960, 730);
        MinimumSize = new Size(880, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        Font = Theme.Label;

        var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Theme.Ink };
        header.Controls.Add(new Label
        {
            Text = "Fino Setup",
            Font = new Font("Segoe UI Semibold", 15F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(28, 14),
            BackColor = Color.Transparent
        });
        _headerSubtitle = new Label
        {
            Text = "checking this server...",
            Font = Theme.Label,
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(30, 48),
            BackColor = Color.Transparent
        };
        header.Controls.Add(_headerSubtitle);

        header.Controls.Add(new Label
        {
            Text = Environment.MachineName,
            Font = Theme.Small,
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Location = new Point(Width - 200, 50),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.Transparent
        });

        _content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 22, 28, 16), AutoScroll = true };

        // The buttons live in a bar of their own, outside the scrolling area. On a long form -
        // and on a 768-pixel-high server console - "Install now" must never be somewhere the
        // person has to scroll to find.
        _footer = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Theme.Card, Visible = false };
        _footer.Paint += (sender, e) =>
        {
            using var pen = new Pen(Theme.Line);
            e.Graphics.DrawLine(pen, 0, 0, ((Control)sender!).Width, 0);
        };

        Controls.Add(_content);
        Controls.Add(_footer);
        Controls.Add(header);
    }

    private readonly Panel _footer;

    /// <summary>Puts the screen's buttons in the fixed bar at the bottom, laid out left to right.</summary>
    private void SetFooter(params Control[] buttons)
    {
        foreach (Control existing in _footer.Controls) existing.Dispose();
        _footer.Controls.Clear();

        if (buttons.Length == 0) { _footer.Visible = false; return; }

        var row = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Location = new Point(28, 14),
            BackColor = Color.Transparent,
            WrapContents = false
        };
        foreach (var button in buttons) row.Controls.Add(button);

        _footer.Controls.Add(row);
        _footer.Visible = true;
    }

    /// <summary>
    /// Set by --screenshot: do not probe the server, the screens are driven by sample data.
    /// A field rather than a property so the Windows Forms analyser does not ask for designer
    /// serialization metadata on something the designer never sees.
    /// </summary>
    internal bool ScreenshotMode;

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (ScreenshotMode) return;
        await RefreshAndShowHomeAsync();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_running is { IsCancellationRequested: false })
        {
            var answer = MessageBox.Show(this,
                "Something is still running. Closing now could leave this server half-set-up.\n\nClose anyway?",
                "Fino Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) { e.Cancel = true; return; }
            _running.Cancel();
        }
        base.OnFormClosing(e);
    }

    // =======================================================================
    // Screen plumbing
    // =======================================================================
    private void ShowScreen(Control screen)
    {
        _content.SuspendLayout();
        foreach (Control existing in _content.Controls) existing.Dispose();
        _content.Controls.Clear();
        screen.Dock = DockStyle.Fill;
        _content.Controls.Add(screen);
        _content.ResumeLayout();
    }

    private static FlowLayoutPanel Column(int width = 860) => new()
    {
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Width = width,
        BackColor = Color.Transparent
    };

    // =======================================================================
    // Loading / probing
    // =======================================================================
    private async Task RefreshAndShowHomeAsync()
    {
        ShowScreen(BuildMessageScreen("Checking this server...",
            "Looking at Windows, memory, disk space and SQL Server, and whether Fino is already installed here."));

        if (!ScriptRunner.HasScripts)
        {
            ShowScreen(BuildMessageScreen("This is not a release folder",
                "FinoSetup.exe has to sit next to Setup.ps1 and the app folder, inside the unzipped release.\n\n" +
                "Unzip the whole release and run FinoSetup.exe from inside the unzipped folder. " +
                "Running it from inside the .zip window does not work - Windows only unpacks the one file you open.",
                Theme.Danger));
            return;
        }

        try
        {
            _probe = await ScriptRunner.ProbeAsync();
            _headerSubtitle.Text = _probe.HasApplication
                ? $"release {_probe.ReleaseVersion}"
                : "no application files found in this folder";
            ShowHome();
        }
        catch (Exception exception)
        {
            ShowScreen(BuildMessageScreen("The server check failed",
                exception.Message, Theme.Danger));
        }
    }

    private Control BuildMessageScreen(string title, string message, Color? accent = null)
    {
        SetFooter();
        var column = Column();
        column.Controls.Add(Ui.Heading(title, Theme.H1, accent ?? Theme.Ink));
        column.Controls.Add(Spacer(10));
        column.Controls.Add(Ui.Body(message, Theme.Body, 760));
        return column;
    }

    // Margin is zeroed everywhere (see UiKit) so that vertical rhythm comes only from these
    // spacers - Windows Forms default margins otherwise add three pixels per control, which adds
    // up to a scrollbar on a form that was measured to fit.
    private static Panel Spacer(int height) => new() { Height = height, Width = 10, Margin = Padding.Empty, BackColor = Color.Transparent };

    // =======================================================================
    // Home
    // =======================================================================
    private void ShowHome()
    {
        var probe = _probe!;
        var installed = probe.Installed;
        var column = Column();

        string title;
        string subtitle;
        if (installed is null)
        {
            title = "Ready to install";
            subtitle = "Nothing is installed on this server yet. Setup will install everything, including the database.";
        }
        else if (installed.IsLegacyIis)
        {
            title = "An older deployment was found";
            subtitle = $"An IIS site is serving Fino from {installed.InstallPath}. It can be moved onto the new setup, keeping its data.";
        }
        else
        {
            var name = string.IsNullOrWhiteSpace(installed.BrandName) ? "Fino" : installed.BrandName;
            title = $"{name} is installed on this server";
            subtitle = probe.VersionCompare switch
            {
                > 0 => $"Installed version {installed.Version} - this release is {probe.ReleaseVersion}. An update is available.",
                0   => $"Version {installed.Version} - the same as this release.",
                < 0 => $"Installed version {installed.Version} is NEWER than this release ({probe.ReleaseVersion}).",
                _   => $"Installed at {installed.InstallPath}."
            };
        }

        column.Controls.Add(Ui.Heading(title));
        column.Controls.Add(Spacer(6));
        column.Controls.Add(Ui.Body(subtitle, Theme.Muted, 780));

        // A dormant service on a box that is also running IIS sites is the shape of "this server
        // has moved to another hosting". Updating it would start a second application against the
        // same database, and two of those can debit one wallet at the same time.
        if (installed is { LooksDormant: true })
        {
            column.Controls.Add(Spacer(12));
            var warning = Ui.Body(
                $"This installation's service is {installed.ServiceState?.ToLowerInvariant()}" +
                (string.Equals(installed.ServiceStart, "Disabled", StringComparison.OrdinalIgnoreCase) ? " and disabled" : "") +
                ". Something else may be serving this database now (an IIS site, for example). " +
                "Check that before updating it - two applications on one database can debit the same wallet twice.",
                Theme.Warning, 780);
            column.Controls.Add(warning);
        }

        column.Controls.Add(Spacer(18));
        column.Controls.Add(BuildServerCard(probe));

        ShowScreen(column);
        SetFooter(BuildActions(probe));
    }

    private Control BuildServerCard(ProbeResult probe)
    {
        var card = Ui.BorderedCard();
        card.Width = 860;
        // Grows with the number of SQL instances found rather than leaving a fixed block of empty
        // white under a short list.
        card.Height = 128 + Math.Max(1, probe.SqlInstances.Count) * 30;

        var inner = Column(800);
        inner.Location = new Point(20, 18);

        inner.Controls.Add(Ui.Heading("This server", Theme.H2));
        inner.Controls.Add(Spacer(8));

        inner.Controls.Add(Ui.StatusLine(probe.Is64Bit ? Theme.Success : Theme.Danger, probe.Os));
        inner.Controls.Add(Ui.StatusLine(
            probe.RamGb >= 3.5 ? Theme.Success : Theme.Warning,
            $"{probe.RamGb:0.#} GB memory",
            probe.RamGb >= 3.5 ? null : "4 GB is the realistic minimum with SQL Server"));
        inner.Controls.Add(Ui.StatusLine(
            probe.FreeDiskGb >= 15 ? Theme.Success : Theme.Warning,
            $"{probe.FreeDiskGb:0.#} GB free disk",
            probe.FreeDiskGb >= 15 ? null : "about 15 GB is needed to install comfortably"));

        if (probe.SqlInstances.Count == 0)
        {
            inner.Controls.Add(Ui.StatusLine(Theme.Warning, "SQL Server: not installed",
                "setup can download and install the free Express edition"));
        }
        else
        {
            foreach (var instance in probe.SqlInstances)
            {
                inner.Controls.Add(Ui.StatusLine(
                    string.Equals(instance.Status, "Running", StringComparison.OrdinalIgnoreCase) ? Theme.Success : Theme.Warning,
                    $"SQL Server {instance.ServerName}", instance.Status.ToLowerInvariant()));
            }
        }

        inner.Controls.Add(Spacer(6));
        inner.Controls.Add(Ui.Body("IIS, .NET and Node.js are not needed - this build hosts itself and brings its own runtime.",
            Theme.Muted, 760));

        card.Controls.Add(inner);
        return card;
    }

    private Control[] BuildActions(ProbeResult probe)
    {
        var installed = probe.Installed;

        if (installed is null)
        {
            var install = Ui.PrimaryButton("Install on this server", 240);
            install.Click += (_, _) => ShowInstallForm();
            return [install];
        }

        if (installed.IsLegacyIis)
        {
            var migrate = Ui.PrimaryButton("Move it to the new setup", 240);
            migrate.Click += (_, _) => StartMigration(installed);

            var fresh = Ui.SecondaryButton("Install separately instead", 220);
            fresh.Click += (_, _) => ShowInstallForm();
            return [migrate, fresh];
        }

        var update = Ui.PrimaryButton(probe.VersionCompare > 0 ? "Update to this release" : "Reinstall this release", 220);
        update.Click += (_, _) => StartUpdate(installed, probe);

        var branding = Ui.SecondaryButton("Change logo / name", 190);
        branding.Click += (_, _) => ShowBrandingForm(installed);

        var backup = Ui.SecondaryButton("Back up now", 150);
        backup.Click += (_, _) => StartBackup(installed);

        var open = Ui.SecondaryButton("Open the panel", 160);
        open.Click += (_, _) => OpenUrl(string.IsNullOrWhiteSpace(installed.PublicUrl) ? installed.LocalUrl : installed.PublicUrl!);

        return [update, branding, backup, open];
    }

    // =======================================================================
    // Install form
    // =======================================================================
    private void ShowInstallForm()
    {
        var probe = _probe!;
        var column = Column();

        // A compact heading here rather than the usual big one: the three cards below already say
        // what each section is, and on a 768-pixel console every line spent on decoration is a line
        // the person has to scroll past to reach the fields.
        var pageHeading = Ui.Heading("Client details - everything else is set up automatically", Theme.H2);
        column.Controls.Add(pageHeading);
        column.Controls.Add(Spacer(12));

        // --- the client -----------------------------------------------------
        var brandField = Ui.Field(340);
        var logoPathLabel = Ui.Body("No file chosen", Theme.Muted, 210);
        var logoPreview = new PictureBox
        {
            Size = new Size(150, 72),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(241, 245, 249)
        };
        string? logoPath = null;

        var chooseLogo = Ui.SecondaryButton("Choose logo file...", 170);
        chooseLogo.Height = 30;
        chooseLogo.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Choose the client's logo",
                Filter = "Logo images (*.png;*.jpg;*.jpeg;*.svg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.svg;*.bmp;*.gif;*.tif;*.tiff|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            if (!TryAcceptLogo(dialog.FileName, logoPreview, logoPathLabel)) return;
            logoPath = dialog.FileName;
        };

        // Card 1 - the client. Company name on the left, logo on the right.
        var clientCard = Ui.BorderedCard();
        clientCard.Width = 860;
        clientCard.Height = 156;   // two-line logo hint under the file name

        var clientHead = Ui.Heading("The client", Theme.H2);
        clientHead.Location = new Point(20, 16);
        clientCard.Controls.Add(clientHead);

        var brandHost = Ui.LabelledField("Company name", brandField, "Shown in the panel and on the public site", 360);
        brandHost.Location = new Point(20, 48);
        clientCard.Controls.Add(brandHost);

        var logoLabel = new Label
        {
            Text = "Logo", Font = Theme.Strong, ForeColor = Theme.Ink, AutoSize = true,
            Location = new Point(430, 48), BackColor = Color.Transparent
        };
        clientCard.Controls.Add(logoLabel);

        logoPreview.Location = new Point(430, 68);
        clientCard.Controls.Add(logoPreview);

        chooseLogo.Location = new Point(592, 68);
        clientCard.Controls.Add(chooseLogo);

        logoPathLabel.Location = new Point(592, 104);
        clientCard.Controls.Add(logoPathLabel);

        // Worth saying out loud: the logo is not only shown, it decides the portal's colours.
        // Otherwise the installer wonders where to set them, and there is nowhere - by design.
        var logoHint = Ui.Body("PNG, JPG or SVG - optional.\nThe portal's colours are taken from it.", Theme.Muted, 240);
        logoHint.Location = new Point(592, 120);
        logoHint.Font = Theme.Small;
        clientCard.Controls.Add(logoHint);

        column.Controls.Add(clientCard);
        column.Controls.Add(Spacer(10));

        // Card 2 - the first login, four fields on one row.
        var mobileField = Ui.Field(150);
        var emailField = Ui.Field(230);
        var passwordField = Ui.Field(150, password: true);
        var confirmField = Ui.Field(150, password: true);

        var loginCard = Ui.BorderedCard();
        loginCard.Width = 860;
        loginCard.Height = 122;

        var loginHead = Ui.Heading("The client's owner login", Theme.H2);
        loginHead.Location = new Point(20, 16);
        loginCard.Controls.Add(loginHead);

        var loginHint = Ui.Body("They are asked to change this password the first time they sign in.", Theme.Muted, 700);
        loginHint.Location = new Point(20, 40);
        loginCard.Controls.Add(loginHint);

        var loginFields = new (string Caption, Control Field, int X, int Width)[]
        {
            ("Mobile number",  mobileField,   0,   160),
            ("Email address",  emailField,    175, 240),
            ("Password",       passwordField, 430, 160),
            ("Password again", confirmField,  600, 160)
        };
        foreach (var (caption, field, x, width) in loginFields)
        {
            var host = Ui.LabelledField(caption, field, null, width);
            host.Location = new Point(20 + x, 64);
            loginCard.Controls.Add(host);
        }

        column.Controls.Add(loginCard);
        column.Controls.Add(Spacer(10));

        // Card 3 - where it runs.
        var urlField = Ui.Field(290);
        var portField = Ui.Field(80);
        portField.Text = "8080";
        var databaseField = Ui.Field(170);
        databaseField.Text = "FinoApp";
        var folderField = Ui.Field(190);
        folderField.Text = @"C:\Fino";

        var sqlCombo = new ComboBox
        {
            Width = 210,
            Font = Theme.Text,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        foreach (var instance in probe.SqlInstances) sqlCombo.Items.Add(instance.ServerName);
        if (probe.SqlInstances.Count == 0) sqlCombo.Items.Add("Install SQL Server Express");
        sqlCombo.SelectedIndex = 0;

        var hostingCard = Ui.BorderedCard();
        hostingCard.Width = 860;
        hostingCard.Height = 178;

        var hostingHead = Ui.Heading("Where it runs", Theme.H2);
        hostingHead.Location = new Point(20, 16);
        hostingCard.Controls.Add(hostingHead);

        var hostingRow1 = new (string Caption, Control Field, string? Hint, int X, int Width)[]
        {
            ("Panel address",   urlField,  "Leave blank if they will use the IP address", 0,   310),
            ("Port",            portField, "the panel's port, NOT the database port",       330, 130),
            ("Database server", sqlCombo,  null,                                          440, 220)
        };
        foreach (var (caption, field, hint, x, width) in hostingRow1)
        {
            var host = Ui.LabelledField(caption, field, hint, width);
            host.Location = new Point(20 + x, 46);
            hostingCard.Controls.Add(host);
        }

        var hostingRow2 = new (string Caption, Control Field, int X, int Width)[]
        {
            ("Database name", databaseField, 0,   180),
            ("Install folder", folderField,  200, 200)
        };
        foreach (var (caption, field, x, width) in hostingRow2)
        {
            var host = Ui.LabelledField(caption, field, null, width);
            host.Location = new Point(20 + x, 124);
            hostingCard.Controls.Add(host);
        }

        column.Controls.Add(hostingCard);

        // --- go ---------------------------------------------------------------
        var back = Ui.SecondaryButton("Back", 110);
        back.Click += (_, _) => ShowHome();

        var start = Ui.PrimaryButton("Install now", 200);
        start.Click += (_, _) =>
        {
            var answers = new InstallAnswers
            {
                Mode = "install",
                BrandName = brandField.Text.Trim(),
                LogoPath = logoPath,
                AdminMobile = mobileField.Text.Trim(),
                AdminEmail = emailField.Text.Trim(),
                AdminPassword = passwordField.Text,
                PublicUrl = urlField.Text.Trim(),
                InstallPath = folderField.Text.Trim(),
                DatabaseName = databaseField.Text.Trim(),
                SqlServer = probe.SqlInstances.Count > 0 ? (sqlCombo.SelectedItem?.ToString() ?? "") : ""
            };

            if (!Validate(answers, portField.Text, confirmField.Text, out var port, out var problem))
            {
                MessageBox.Show(this, problem, "Check these first", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            answers.Port = port;

            if (probe.SqlInstances.Count == 0)
            {
                var proceed = MessageBox.Show(this,
                    "This server has no database engine, so SQL Server Express will be downloaded and installed first.\n\n" +
                    "That needs internet access and takes 10 to 20 minutes.\n\nContinue?",
                    "SQL Server needed", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (proceed != DialogResult.OK) return;
            }

            StartRun(answers, "Installing", installed: null);
        };

        ShowScreen(column);
        SetFooter(back, start);
        brandField.Focus();
    }

    /// <summary>
    /// Checks a chosen logo and previews it. Design-tool formats are refused here, with the
    /// instructions to give the client's designer, rather than failing later in the install.
    /// </summary>
    private bool TryAcceptLogo(string path, PictureBox preview, Label pathLabel)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        string[] design = [".cdr", ".ai", ".eps", ".psd", ".indd", ".pdf", ".webp", ".sketch", ".fig", ".xd"];
        string[] raster = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff"];

        if (design.Contains(extension))
        {
            var name = extension.TrimStart('.').ToUpperInvariant();
            var how = extension switch
            {
                ".cdr" => "In CorelDRAW:  File -> Export -> choose PNG",
                ".ai" or ".eps" => "In Illustrator:  File -> Export -> Export As -> PNG",
                ".psd" => "In Photoshop:  File -> Export -> Quick Export as PNG",
                ".pdf" => "Open the PDF and export the logo as PNG",
                _ => "Export or \"Save as\" PNG from whichever program made the file"
            };

            MessageBox.Show(this,
                $"A {name} file cannot be used on a web page.\n\n" +
                $"Ask the client's designer for a PNG instead:\n    {how}\n" +
                "    Roughly 512 x 512 pixels, transparent background if possible.\n\n" +
                "An SVG also works.\n\n" +
                "You can install now without a logo and add it later with \"Change logo / name\" - " +
                "nothing has to be reinstalled.",
                "That file format will not work", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (extension == ".svg")
        {
            preview.Image = null;
            preview.BackColor = Color.FromArgb(226, 240, 253);
            pathLabel.Text = Path.GetFileName(path) + "  (SVG - stays sharp at any size)";
            pathLabel.ForeColor = Theme.Body;
            return true;
        }

        if (!raster.Contains(extension))
        {
            MessageBox.Show(this, "Choose a PNG, JPG or SVG file.", "Unsupported file",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        try
        {
            // Loaded through a copy in memory so the file is not left locked by the preview.
            using var stream = new MemoryStream(File.ReadAllBytes(path));
            preview.Image = Image.FromStream(stream);
            preview.BackColor = Color.FromArgb(241, 245, 249);
            pathLabel.Text = Path.GetFileName(path);
            pathLabel.ForeColor = Theme.Body;
            return true;
        }
        catch
        {
            MessageBox.Show(this, "That image could not be opened. It may be damaged - ask for another copy.",
                "Cannot read the image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }

    private static bool Validate(InstallAnswers answers, string portText, string confirmPassword,
                                 out int port, out string problem)
    {
        port = 0;
        problem = "";

        if (string.IsNullOrWhiteSpace(answers.BrandName)) { problem = "The company name is needed - it is what the panel is called."; return false; }

        var mobile = new string(answers.AdminMobile.Where(char.IsDigit).ToArray());
        if (mobile.Length != 10) { problem = "The mobile number should be 10 digits - that is what the login screen asks for."; return false; }
        answers.AdminMobile = mobile;

        if (answers.AdminPassword.Length < 8) { problem = "The password needs at least 8 characters."; return false; }
        if (answers.AdminPassword != confirmPassword) { problem = "The two passwords are not the same."; return false; }

        if (!int.TryParse(portText.Trim(), out port) || port is < 1 or > 65535) { problem = "The port must be a number between 1 and 65535."; return false; }

        // Caught here as well as in the script, so the person sees it while the field is still in
        // front of them. 1433 has been entered here for real - it is SQL Server's port, and an
        // installation configured with it can never start.
        var reserved = new Dictionary<int, string>
        {
            [53] = "DNS", [88] = "Kerberos", [135] = "Windows RPC", [139] = "NetBIOS",
            [389] = "Active Directory", [445] = "Windows file sharing",
            [1433] = "SQL Server - this is the database's own port, not the panel's",
            [1434] = "SQL Server Browser", [3306] = "MySQL",
            [3389] = "Remote Desktop - taking it would lock you out of this server",
            [5432] = "PostgreSQL", [5985] = "Windows Remote Management", [5986] = "Windows Remote Management",
            [6379] = "Redis", [27017] = "MongoDB"
        };
        if (reserved.TryGetValue(port, out var owner))
        {
            problem = $"Port {port} belongs to {owner}.\n\nUse a port of the panel's own - 8080, 8081 or 9090 are the usual choices.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(answers.InstallPath)) { problem = "An install folder is needed."; return false; }
        if (string.IsNullOrWhiteSpace(answers.DatabaseName)) { problem = "A database name is needed."; return false; }

        if (!string.IsNullOrWhiteSpace(answers.PublicUrl) &&
            !answers.PublicUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !answers.PublicUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            answers.PublicUrl = "https://" + answers.PublicUrl.Trim();
        }

        return true;
    }

    // =======================================================================
    // Branding
    // =======================================================================
    private void ShowBrandingForm(InstalledInfo installed)
    {
        var column = Column();
        column.Controls.Add(Ui.Heading("Logo and company name"));
        column.Controls.Add(Spacer(4));
        column.Controls.Add(Ui.Body(
            "Changes are live as soon as the client refreshes the page. Nothing is reinstalled and the service is not restarted.",
            Theme.Muted, 780));
        column.Controls.Add(Spacer(18));

        var brandField = Ui.Field(340);
        brandField.Text = installed.BrandName ?? "";

        var preview = new PictureBox { Size = new Size(150, 72), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(241, 245, 249) };
        var pathLabel = Ui.Body("Keep the current logo", Theme.Muted, 210);
        string? logoPath = null;

        var choose = Ui.SecondaryButton("Choose logo file...", 170);
        choose.Height = 30;
        choose.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Choose the client's logo",
                Filter = "Logo images (*.png;*.jpg;*.jpeg;*.svg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.svg;*.bmp;*.gif;*.tif;*.tiff|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (!TryAcceptLogo(dialog.FileName, preview, pathLabel)) return;
            logoPath = dialog.FileName;
        };

        var card = Ui.BorderedCard();
        card.Width = 860;
        card.Height = 170;

        var left = Column(360);
        left.Location = new Point(20, 18);
        left.Controls.Add(Ui.LabelledField("Company name", brandField));
        card.Controls.Add(left);

        var right = new Panel { Location = new Point(430, 18), Width = 400, Height = 120, BackColor = Color.Transparent };
        right.Controls.Add(new Label { Text = "Logo", Font = Theme.Strong, ForeColor = Theme.Ink, AutoSize = true, Location = new Point(0, 0), BackColor = Color.Transparent });
        preview.Location = new Point(0, 22);
        right.Controls.Add(preview);
        choose.Location = new Point(162, 22);
        right.Controls.Add(choose);
        pathLabel.Location = new Point(162, 58);
        right.Controls.Add(pathLabel);
        card.Controls.Add(right);

        column.Controls.Add(card);

        var back = Ui.SecondaryButton("Back", 110);
        back.Click += (_, _) => ShowHome();

        var apply = Ui.PrimaryButton("Apply", 160);
        apply.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(brandField.Text))
            {
                MessageBox.Show(this, "The company name cannot be blank.", "Check these first", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var log = ShowRunning("Applying the branding", "This takes a moment.");
            var exitCode = await RunGuardedAsync(token =>
                ScriptRunner.RunBrandingAsync(installed.InstallPath!, brandField.Text.Trim(), logoPath, log, token));

            if (exitCode == 0)
            {
                ShowDone("Branding updated",
                    "Ask the client to press Ctrl+F5 once in their browser - the panel picks it up on the next refresh.",
                    installed);
            }
            else
            {
                ShowFailed("The branding could not be applied", "The details are in the log above.");
            }
        };

        ShowScreen(column);
        SetFooter(back, apply);
    }

    // =======================================================================
    // Update / migrate / backup
    // =======================================================================
    private void StartUpdate(InstalledInfo installed, ProbeResult probe)
    {
        if (probe.VersionCompare < 0)
        {
            var answer = MessageBox.Show(this,
                $"Installed version {installed.Version} is NEWER than this release ({probe.ReleaseVersion}).\n\n" +
                "Going backwards is not normally what you want.\n\nContinue anyway?",
                "Older release", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;
        }

        if (installed.LooksDormant)
        {
            var dormant = MessageBox.Show(this,
                $"This installation's service is {installed.ServiceState?.ToLowerInvariant()}" +
                (string.Equals(installed.ServiceStart, "Disabled", StringComparison.OrdinalIgnoreCase) ? " and disabled" : "") +
                ".\n\n" +
                "That usually means this server now serves the platform some other way - IIS sites, for example - " +
                "and this copy is dormant.\n\n" +
                "Updating it will start it again against the same database. Two applications on one database " +
                "can debit the same wallet at the same time.\n\n" +
                "Are you sure this is the copy that should be running?",
                "This copy looks dormant", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dormant != DialogResult.Yes) return;
        }

        var confirm = MessageBox.Show(this,
            "The update will:\n\n" +
            "    1.  back up the database, the uploaded files and the keys\n" +
            "    2.  stop the service for a few minutes\n" +
            "    3.  replace the program files, keeping settings, uploads and branding\n" +
            "    4.  put the previous version back automatically if the new one does not start\n\n" +
            "The platform is offline while this runs. Continue?",
            "Update", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        if (confirm != DialogResult.OK) return;

        var answers = new InstallAnswers
        {
            Mode = "update",
            InstallPath = installed.InstallPath ?? @"C:\Fino",
            ServiceName = installed.ServiceName ?? "FinoAPI"
        };
        StartRun(answers, "Updating", installed);
    }

    private void StartMigration(InstalledInfo installed)
    {
        var confirm = MessageBox.Show(this,
            $"The IIS deployment at {installed.InstallPath} will be moved onto the new setup.\n\n" +
            "    - the SAME database is used, so no data moves and nothing is lost\n" +
            "    - the existing keys are carried over, so nobody is signed out\n" +
            "    - the database is backed up first\n" +
            "    - the old IIS sites are left running; stop them from IIS Manager once you have checked the new address\n\n" +
            "The platform is offline for a few minutes. Do this in a maintenance window.\n\nContinue?",
            "Move to the new setup", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
        if (confirm != DialogResult.OK) return;

        var brand = Prompt("Company name", "What should the panel be called?", installed.BrandName ?? "");
        if (brand is null) return;

        var answers = new InstallAnswers
        {
            Mode = "migrate",
            BrandName = brand,
            Port = 8080,
            InstallPath = @"C:\Fino"
        };
        StartRun(answers, "Moving the deployment", installed, confirmMigration: true);
    }

    private async void StartBackup(InstalledInfo installed)
    {
        var log = ShowRunning("Backing up", "The database, the uploaded files and this installation's keys.");
        var exitCode = await RunGuardedAsync(token => ScriptRunner.RunBackupAsync(installed.InstallPath!, log, token));

        if (exitCode == 0)
        {
            ShowDone("Backup complete",
                "It is in C:\\FinoBackups. Copy it off this server - a backup on the same machine does not survive that machine.",
                installed);
        }
        else
        {
            ShowFailed("The backup failed", "The details are in the log above.");
        }
    }

    private async void StartRun(InstallAnswers answers, string what, InstalledInfo? installed, bool confirmMigration = false)
    {
        var subtitle = answers.Mode switch
        {
            "install" => "Creating the database and starting the service. A new database takes 3 to 5 minutes to build.",
            "update"  => "Backing up, swapping the files, and checking that the new build comes up.",
            _         => "Backing up, installing the new hosting, and pointing it at the same database."
        };

        var log = ShowRunning(what, subtitle);
        if (confirmMigration) answers.Mode = "migrate";

        var exitCode = await RunGuardedAsync(token => ScriptRunner.RunSetupAsync(answers, log, token));

        if (exitCode == 0)
        {
            var address = !string.IsNullOrWhiteSpace(answers.PublicUrl)
                ? answers.PublicUrl
                : $"http://localhost:{answers.Port}";

            var target = installed ?? new InstalledInfo
            {
                Mode = "service",
                InstallPath = answers.InstallPath,
                ServiceName = answers.ServiceName,
                BrandName = answers.BrandName,
                Port = answers.Port,
                Scheme = "http",
                PublicUrl = answers.PublicUrl
            };

            ShowDone(
                answers.Mode == "update" ? "Update complete" : "Installation complete",
                answers.Mode == "update"
                    ? "The new version is live and the platform is back up."
                    : $"The panel is at {address}. Sign in with the mobile number and password you entered, then change that password.",
                target);
        }
        else
        {
            ShowFailed(
                answers.Mode == "update" ? "The update did not finish" : "The installation did not finish",
                "Read the last red line in the log above - it says which step failed and why. " +
                "Nothing else has to be undone by hand; fix that one thing and run this again.");
        }
    }

    // =======================================================================
    // Running screen
    // =======================================================================
    private RichTextBox? _log;
    private Label? _runningStatus;

    private Action<string> ShowRunning(string title, string subtitle)
    {
        var column = Column();
        column.Controls.Add(Ui.Heading(title));
        column.Controls.Add(Spacer(4));
        column.Controls.Add(Ui.Body(subtitle, Theme.Muted, 780));
        column.Controls.Add(Spacer(14));

        _runningStatus = new Label
        {
            Text = "Starting...",
            Font = Theme.Strong,
            ForeColor = Theme.Accent,
            AutoSize = false,
            Size = new Size(820, 22),
            BackColor = Color.Transparent
        };
        column.Controls.Add(_runningStatus);

        column.Controls.Add(new ProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30,
            Width = 860,
            Height = 6
        });
        column.Controls.Add(Spacer(12));

        _log = new RichTextBox
        {
            Width = 860,
            Height = 330,
            ReadOnly = true,
            BackColor = Theme.Terminal,
            ForeColor = Color.FromArgb(203, 213, 225),
            Font = Theme.Mono,
            BorderStyle = BorderStyle.None,
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Both
        };
        column.Controls.Add(_log);

        ShowScreen(column);
        SetFooter();   // nothing to press while it works

        return line => BeginInvoke(() => AppendLog(line));
    }

    private void AppendLog(string line)
    {
        if (_log is null || _log.IsDisposed) return;

        var colour = Color.FromArgb(203, 213, 225);
        if (line.Contains("[OK]")) colour = Color.FromArgb(134, 239, 172);
        else if (line.Contains("[FAIL]")) colour = Color.FromArgb(252, 165, 165);
        else if (line.Contains("[!]")) colour = Color.FromArgb(253, 224, 71);
        else if (line.StartsWith("===")) colour = Color.FromArgb(125, 211, 252);

        _log.SelectionStart = _log.TextLength;
        _log.SelectionColor = colour;
        _log.AppendText(line + Environment.NewLine);
        _log.ScrollToCaret();

        // "=== Writing the configuration" is the script announcing a step; that is the one line
        // worth showing large, for someone who is not reading the log.
        var step = Regex.Match(line, @"^===\s*(.+)$");
        if (step.Success && _runningStatus is { IsDisposed: false })
        {
            _runningStatus.Text = step.Groups[1].Value.Trim();
        }
    }

    private async Task<int> RunGuardedAsync(Func<CancellationToken, Task<int>> work)
    {
        _running = new CancellationTokenSource();
        try
        {
            return await work(_running.Token);
        }
        catch (Exception exception)
        {
            AppendLog("[FAIL] " + exception.Message);
            return -1;
        }
        finally
        {
            _running.Dispose();
            _running = null;
        }
    }

    // =======================================================================
    // Done / failed
    // =======================================================================
    private void ShowDone(string title, string message, InstalledInfo? installed)
    {
        var column = Column();
        column.Controls.Add(Ui.Heading(title, Theme.H1, Theme.Success));
        column.Controls.Add(Spacer(6));
        column.Controls.Add(Ui.Body(message, Theme.Body, 780));
        column.Controls.Add(Spacer(14));

        var buttons = new List<Control>();

        if (installed is not null)
        {
            var open = Ui.PrimaryButton("Open the panel", 180);
            open.Click += (_, _) => OpenUrl(string.IsNullOrWhiteSpace(installed.PublicUrl) ? installed.LocalUrl : installed.PublicUrl!);
            buttons.Add(open);

            var summaryPath = Path.Combine(installed.InstallPath ?? "", "INSTALLATION-SUMMARY.txt");
            if (File.Exists(summaryPath))
            {
                var summary = Ui.SecondaryButton("Open the summary", 180);
                summary.Click += (_, _) => OpenUrl(summaryPath);
                buttons.Add(summary);
            }
        }

        var home = Ui.SecondaryButton("Back to start", 150);
        home.Click += async (_, _) => await RefreshAndShowHomeAsync();
        buttons.Add(home);

        var close = Ui.SecondaryButton("Close", 110);
        close.Click += (_, _) => Close();
        buttons.Add(close);

        if (_log is not null && !_log.IsDisposed)
        {
            var text = _log.Text;
            var copy = new RichTextBox
            {
                Width = 860, Height = 300, ReadOnly = true,
                BackColor = Theme.Terminal, ForeColor = Color.FromArgb(148, 163, 184),
                Font = Theme.Mono, BorderStyle = BorderStyle.None, WordWrap = false,
                Text = text
            };
            column.Controls.Add(copy);
        }

        ShowScreen(column);
        SetFooter(buttons.ToArray());
    }

    private void ShowFailed(string title, string message)
    {
        var column = Column();
        column.Controls.Add(Ui.Heading(title, Theme.H1, Theme.Danger));
        column.Controls.Add(Spacer(6));
        column.Controls.Add(Ui.Body(message, Theme.Body, 780));
        column.Controls.Add(Spacer(14));

        var saveLog = Ui.PrimaryButton("Save the log to a file", 200);
        saveLog.Click += (_, _) =>
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Save the setup log",
                FileName = $"fino-setup-{DateTime.Now:yyyyMMdd-HHmm}.log",
                Filter = "Log files (*.log)|*.log|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            File.WriteAllText(dialog.FileName, _log?.Text ?? "");
            MessageBox.Show(this, "Saved. Send this file to support.", "Fino Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        var home = Ui.SecondaryButton("Back to start", 150);
        home.Click += async (_, _) => await RefreshAndShowHomeAsync();

        if (_log is not null && !_log.IsDisposed)
        {
            var copy = new RichTextBox
            {
                Width = 860, Height = 330, ReadOnly = true,
                BackColor = Theme.Terminal, ForeColor = Color.FromArgb(203, 213, 225),
                Font = Theme.Mono, BorderStyle = BorderStyle.None, WordWrap = false,
                Text = _log.Text
            };
            column.Controls.Add(copy);
        }

        ShowScreen(column);
        SetFooter(saveLog, home);
    }

    /// <summary>
    /// Builds one screen with sample data and lays it out, without showing the window or touching
    /// the server. Used by --screenshot so the layout can be reviewed like any other change.
    /// </summary>
    internal void RenderForScreenshot(ProbeResult probe, string screen)
    {
        _probe = probe;
        _headerSubtitle.Text = $"release {probe.ReleaseVersion}";

        switch (screen)
        {
            case "install":
                ShowInstallForm();
                break;

            case "running":
                var log = ShowRunning("Installing",
                    "Creating the database and starting the service. A new database takes 3 to 5 minutes to build.");
                foreach (var line in new[]
                {
                    "=== Testing the database connection",
                    "  [OK]   Connected: Microsoft SQL Server 2022 (RTM)",
                    "=== Preparing the database",
                    "  [OK]   Database [RiPay] created",
                    "  [OK]   'NT AUTHORITY\\SYSTEM' (the service account) granted db_owner on [RiPay]",
                    "=== Writing the configuration",
                    "  [OK]   appsettings.Production.json written",
                    "  [OK]   Logo applied (panel, public site and browser tab)",
                    "=== Starting the service",
                    "  The first start creates the database and can take 3-5 minutes. Please wait."
                })
                {
                    AppendLog(line);
                }
                break;

            case "done":
                ShowRunning("Installing", "...");
                AppendLog("=== Starting the service");
                AppendLog("  [OK]   Service is up and the database is ready");
                AppendLog("  [OK]   Nightly backup scheduled for 02:00 (Task Scheduler)");
                ShowDone("Installation complete",
                    "The panel is at https://panel.ripay.co.in. Sign in with the mobile number and password you entered, then change that password.",
                    probe.Installed);
                break;

            case "failed":
                ShowRunning("Installing", "...");
                AppendLog("=== Testing the database connection");
                AppendLog("  [FAIL] Could not connect to SQL Server at 'localhost\\SQLEXPRESS'.");
                ShowFailed("The installation did not finish",
                    "Read the last red line in the log above - it says which step failed and why. " +
                    "Nothing else has to be undone by hand; fix that one thing and run this again.");
                break;

            default:
                ShowHome();
                break;
        }

        // Force a layout pass; DrawToBitmap on a never-shown form otherwise captures empty panels.
        CreateControl();
        PerformLayout();
        _content.PerformLayout();
        Refresh();
    }

    // =======================================================================
    // Small helpers
    // =======================================================================
    private static void OpenUrl(string target)
    {
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
        catch { /* no browser, or the file moved - not worth interrupting for */ }
    }

    private string? Prompt(string title, string question, string initial)
    {
        using var dialog = new Form
        {
            Text = title,
            Size = new Size(460, 190),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Theme.Card
        };

        dialog.Controls.Add(new Label
        {
            Text = question, Font = Theme.Label, ForeColor = Theme.Body,
            Location = new Point(20, 18), Size = new Size(400, 36)
        });

        var field = Ui.Field(400);
        field.Text = initial;
        field.Location = new Point(20, 56);
        dialog.Controls.Add(field);

        var ok = Ui.PrimaryButton("OK", 110);
        ok.Location = new Point(210, 100);
        ok.DialogResult = DialogResult.OK;
        dialog.Controls.Add(ok);

        var cancel = Ui.SecondaryButton("Cancel", 110);
        cancel.Location = new Point(330, 100);
        cancel.DialogResult = DialogResult.Cancel;
        dialog.Controls.Add(cancel);

        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;

        return dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(field.Text)
            ? field.Text.Trim()
            : null;
    }
}
