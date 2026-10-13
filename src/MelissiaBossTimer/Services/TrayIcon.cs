using Forms = System.Windows.Forms;

namespace MelissiaBossTimer;

/// <summary>System tray icon. WPF has no tray API, so this wraps the WinForms NotifyIcon.</summary>
internal sealed class TrayIcon : IDisposable
{
    private readonly Forms.NotifyIcon _icon = new();

    public event Action? OpenRequested;
    public event Action? TestRequested;
    public event Action? ExitRequested;

    public TrayIcon()
    {
        var resource = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/app.ico"))
            ?? throw new InvalidOperationException("app.ico resource missing.");
        using (var stream = resource.Stream)
            _icon.Icon = new System.Drawing.Icon(stream);

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => OpenRequested?.Invoke());
        menu.Items.Add("Test alarm", null, (_, _) => TestRequested?.Invoke());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitRequested?.Invoke());
        _icon.ContextMenuStrip = menu;
        _icon.Text = "Melissia Boss Timer";
        _icon.DoubleClick += (_, _) => OpenRequested?.Invoke();
        _icon.BalloonTipClicked += (_, _) => OpenRequested?.Invoke();
        _icon.Visible = true;
    }

    public void SetTooltip(string text) => _icon.Text = text.Length <= 127 ? text : text[..126] + "…";

    public void Notify(string title, string text, bool warning) =>
        _icon.ShowBalloonTip(15_000, title, text, warning ? Forms.ToolTipIcon.Warning : Forms.ToolTipIcon.Info);

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
