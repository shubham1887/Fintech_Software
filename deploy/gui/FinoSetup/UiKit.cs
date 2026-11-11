using System.Drawing.Drawing2D;

namespace FinoSetup;

/// <summary>
/// The look of the window, in one place.
///
/// Windows Forms defaults (grey 3D buttons, MS Sans Serif) make a 2026 installer look abandoned,
/// and the person running this has to trust it with a payments platform. These are flat controls,
/// one accent colour, and plenty of space.
/// </summary>
public static class Theme
{
    public static readonly Color Ink        = Color.FromArgb(15, 23, 42);    // headers, primary text
    public static readonly Color Body       = Color.FromArgb(51, 65, 85);    // normal text
    public static readonly Color Muted      = Color.FromArgb(100, 116, 139); // hints, secondary
    public static readonly Color Line       = Color.FromArgb(226, 232, 240); // borders
    public static readonly Color Background = Color.FromArgb(248, 250, 252);
    public static readonly Color Card       = Color.White;
    public static readonly Color Accent     = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentDark = Color.FromArgb(29, 78, 216);
    public static readonly Color Success    = Color.FromArgb(22, 163, 74);
    public static readonly Color Warning    = Color.FromArgb(217, 119, 6);
    public static readonly Color Danger     = Color.FromArgb(220, 38, 38);
    public static readonly Color Terminal   = Color.FromArgb(17, 24, 39);

    public static readonly Font H1     = new("Segoe UI Semibold", 17F);
    public static readonly Font H2     = new("Segoe UI Semibold", 12F);
    public static readonly Font Label  = new("Segoe UI", 9.5F);
    public static readonly Font Strong = new("Segoe UI Semibold", 9.5F);
    public static readonly Font Text   = new("Segoe UI", 10F);
    public static readonly Font Small  = new("Segoe UI", 8.75F);
    public static readonly Font Mono   = new("Consolas", 9F);
}

public static class Ui
{
    public static Label Heading(string text, Font? font = null, Color? color = null) => new()
    {
        Text = text,
        Font = font ?? Theme.H1,
        ForeColor = color ?? Theme.Ink,
        AutoSize = true,
        Margin = Padding.Empty,
        BackColor = Color.Transparent
    };

    /// <summary>
    /// Wrapping body text. AutoSize stays on with a maximum width - that is the combination that
    /// makes a Label wrap and then grow to fit. Turning AutoSize off instead leaves the label at its
    /// default height and the text is clipped to one line.
    /// </summary>
    public static Label Body(string text, Color? color = null, int width = 0) => new()
    {
        Text = text,
        Font = Theme.Label,
        ForeColor = color ?? Theme.Body,
        AutoSize = true,
        MaximumSize = width == 0 ? Size.Empty : new Size(width, 0),
        Margin = Padding.Empty,
        BackColor = Color.Transparent
    };

    public static Panel Card() => new()
    {
        BackColor = Theme.Card,
        Padding = new Padding(20),
        Margin = Padding.Empty,
        BorderStyle = BorderStyle.None
    };

    /// <summary>A card with a hairline border - Panel cannot do one-pixel borders on its own.</summary>
    public static Panel BorderedCard()
    {
        var panel = Card();
        panel.Paint += (sender, e) =>
        {
            var control = (Control)sender!;
            using var pen = new Pen(Theme.Line);
            e.Graphics.DrawRectangle(pen, 0, 0, control.Width - 1, control.Height - 1);
        };
        return panel;
    }

    public static Button PrimaryButton(string text, int width = 200) => StyleButton(new Button
    {
        Text = text,
        Width = width,
        Height = 42,
        BackColor = Theme.Accent,
        ForeColor = Color.White
    }, Theme.AccentDark);

    public static Button SecondaryButton(string text, int width = 200)
    {
        var button = StyleButton(new Button
        {
            Text = text,
            Width = width,
            Height = 42,
            BackColor = Color.White,
            ForeColor = Theme.Ink
        }, Color.FromArgb(241, 245, 249));

        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Theme.Line;
        return button;
    }

    private static Button StyleButton(Button button, Color hover)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = Theme.Strong;
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;

        var normal = button.BackColor;
        button.MouseEnter += (_, _) => button.BackColor = hover;
        button.MouseLeave += (_, _) => button.BackColor = normal;
        button.EnabledChanged += (_, _) =>
        {
            button.BackColor = button.Enabled ? normal : Color.FromArgb(226, 232, 240);
            button.ForeColor = button.Enabled ? (normal == Theme.Accent ? Color.White : Theme.Ink) : Theme.Muted;
        };
        return button;
    }

    public static TextBox Field(int width = 320, bool password = false) => new()
    {
        Width = width,
        Font = Theme.Text,
        BorderStyle = BorderStyle.FixedSingle,
        UseSystemPasswordChar = password
    };

    /// <summary>A label, its field and an optional hint, stacked - the form's basic building block.</summary>
    public static Panel LabelledField(string caption, Control field, string? hint = null, int width = 340)
    {
        var host = new Panel { Width = width, Height = hint is null ? 58 : 74, BackColor = Color.Transparent };

        host.Controls.Add(new Label
        {
            Text = caption,
            Font = Theme.Strong,
            ForeColor = Theme.Ink,
            AutoSize = true,
            Location = new Point(0, 0),
            BackColor = Color.Transparent
        });

        field.Location = new Point(0, 20);
        if (field is TextBox textBox) textBox.Height = 26;
        host.Controls.Add(field);

        if (hint is not null)
        {
            host.Controls.Add(new Label
            {
                Text = hint,
                Font = Theme.Small,
                ForeColor = Theme.Muted,
                AutoSize = false,
                Size = new Size(width, 28),
                Location = new Point(0, 50),
                BackColor = Color.Transparent
            });
        }

        return host;
    }

    /// <summary>A coloured status dot followed by text - used for the server checks.</summary>
    public static Panel StatusLine(Color dot, string text, string? detail = null, int width = 520)
    {
        var host = new Panel { Width = width, Height = 24, BackColor = Color.Transparent };

        var marker = new Panel
        {
            Size = new Size(9, 9),
            Location = new Point(0, 6),
            BackColor = dot
        };
        marker.Paint += (sender, e) =>
        {
            var control = (Control)sender!;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(control.BackColor);
            e.Graphics.FillEllipse(brush, 0, 0, control.Width - 1, control.Height - 1);
        };
        host.Controls.Add(marker);

        host.Controls.Add(new Label
        {
            Text = text,
            Font = Theme.Label,
            ForeColor = Theme.Body,
            AutoSize = true,
            Location = new Point(18, 2),
            BackColor = Color.Transparent
        });

        if (detail is not null)
        {
            host.Controls.Add(new Label
            {
                Text = detail,
                Font = Theme.Label,
                ForeColor = Theme.Muted,
                AutoSize = true,
                Location = new Point(18 + TextRenderer.MeasureText(text, Theme.Label).Width + 8, 2),
                BackColor = Color.Transparent
            });
        }

        return host;
    }
}
