using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MelissiaBossTimer.Core;

namespace MelissiaBossTimer.Pages;

internal sealed class SettingsPage : UserControl, IPage
{
    private readonly AppState _state;
    private readonly ComboBox _lead = new() { ItemsSource = LeadOption.All, HorizontalAlignment = HorizontalAlignment.Left, MinWidth = 220 };
    private readonly CheckBox _startup;
    private readonly TextBlock _schedule = Ui.Body("");
    private readonly TextBlock _zone = Ui.Body("");
    private bool _updating;

    public SettingsPage(AppState state, MainWindow host)
    {
        _state = state;
        Focusable = false;

        var columns = new Grid();
        columns.ColumnDefinitions.Add(new ColumnDefinition());
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        columns.ColumnDefinitions.Add(new ColumnDefinition());
        var left = new StackPanel();
        var right = new StackPanel();
        Grid.SetColumn(right, 2);
        columns.Children.Add(left);
        columns.Children.Add(right);

        _lead.SelectionChanged += (_, _) =>
        {
            if (_updating || _lead.SelectedItem is not LeadOption lead)
                return;
            _state.Settings.LeadMinutes = lead.Minutes;
            _state.Save();
        };
        System.Windows.Automation.AutomationProperties.SetName(_lead, "Alert lead time");
        left.Children.Add(Block("Alerts",
            Ui.Label("RING THIS LONG BEFORE A SPAWN"),
            _lead,
            Note("Missed alarms while the PC slept are skipped, not played late.")));

        _startup = Ui.Switch("Start with Windows (opens in the tray)", false, on =>
        {
            if (!_updating)
                StartupRegistration.Set(on);
        });
        left.Children.Add(Block("Windows",
            _startup,
            Note("Closing the window keeps the timer running in the tray. Right-click the tray icon and choose Exit to quit.")));

        var refresh = Ui.Button("Refresh now", async (_, _) => await host.RefreshScheduleAsync(), glyph: Ui.Glyph.Refresh);
        var wiki = Ui.Button("Open wiki page", (_, _) => Open(BossSchedule.SourceUrl), primary: false, glyph: Ui.Glyph.Link);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 0) };
        buttons.Children.Add(refresh);
        buttons.Children.Add(new Border { Width = 10 });
        buttons.Children.Add(wiki);
        right.Children.Add(Block("Schedule", _schedule, buttons));

        right.Children.Add(Block("Time zone", _zone));

        var folder = Ui.Button("Open data folder", (_, _) =>
        {
            Directory.CreateDirectory(AppPaths.Folder);
            Open(AppPaths.Folder);
        }, primary: false, glyph: Ui.Glyph.Folder);
        folder.HorizontalAlignment = HorizontalAlignment.Left;
        folder.Margin = new Thickness(0, 12, 0, 0);
        var version = typeof(SettingsPage).Assembly.GetName().Version;
        right.Children.Add(Block("Data",
            Ui.Body($"Settings and your sounds live in {AppPaths.Folder}."),
            folder,
            Note($"Melissia Boss Timer {version?.ToString(3)}. Persona-style homage; not affiliated with Atlus, Pearl Abyss or Melissia Games.")));

        var root = new DockPanel { Margin = new Thickness(22, 18, 22, 0) };
        var title = new TextBlock { Text = "SETTINGS", Style = (Style)Application.Current.FindResource("PageTitle"), Margin = new Thickness(0, 0, 0, 14) };
        DockPanel.SetDock(title, Dock.Top);
        root.Children.Add(title);
        root.Children.Add(new ScrollViewer { Content = columns, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 10, 18) });
        Content = root;
    }

    private static FrameworkElement Block(string title, params FrameworkElement[] children)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 22) };
        stack.Children.Add(Ui.Tab(title));
        var body = new StackPanel { Margin = new Thickness(16, 14, 16, 16) };
        foreach (var child in children)
            body.Children.Add(child);
        stack.Children.Add(new Border { Background = Ui.Brush("Surface"), Margin = new Thickness(0, 4, 0, 0), Child = body });
        return stack;
    }

    private static TextBlock Note(string text)
    {
        var note = Ui.Body(text);
        note.FontSize = 13;
        note.Margin = new Thickness(0, 10, 0, 0);
        return note;
    }

    private static void Open(string target) => Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });

    public void Refresh()
    {
        _updating = true;
        _lead.SelectedItem = LeadOption.All.FirstOrDefault(l => l.Minutes == _state.Settings.LeadMinutes) ?? LeadOption.All[4];
        _startup.IsChecked = StartupRegistration.IsEnabled;
        _updating = false;

        _schedule.Text = $"Using the {_state.Schedule.Description}. The app checks the wiki on start and every 12 hours, and keeps the last good copy if the site is down.";
        var zone = TimeZoneInfo.Local;
        var offset = zone.GetUtcOffset(DateTime.UtcNow);
        _zone.Text = $"Times follow this PC: {zone.DisplayName}, now UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset:hh\\:mm}. The wiki lists GMT; change the Windows time zone and every time here moves with it.";
    }
}
