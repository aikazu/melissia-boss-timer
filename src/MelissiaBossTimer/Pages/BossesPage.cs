using System.Windows;
using System.Windows.Controls;
using MelissiaBossTimer.Core;

namespace MelissiaBossTimer.Pages;

/// <summary>Every boss in the schedule: alarm on/off and an optional sound of its own, soonest first.</summary>
internal sealed class BossesPage : UserControl, IPage
{
    private static readonly GridLength[] Columns =
    [
        new(1, GridUnitType.Star), new(100), new(66), new(100), new(184), new(64),
    ];

    private readonly AppState _state;
    private readonly StackPanel _rows = new();

    public BossesPage(AppState state)
    {
        _state = state;
        Focusable = false;

        var root = new DockPanel { Margin = new Thickness(22, 18, 22, 0) };

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
        var bulk = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        bulk.Children.Add(Ui.Button("All on", (_, _) => SetAll(_ => true), primary: false));
        bulk.Children.Add(Spacer());
        bulk.Children.Add(Ui.Button("Majors only", (_, _) => SetAll(b => !_state.IsRegular(b)), primary: false));
        bulk.Children.Add(Spacer());
        bulk.Children.Add(Ui.Button("All off", (_, _) => SetAll(_ => false), primary: false));
        DockPanel.SetDock(bulk, Dock.Right);
        header.Children.Add(bulk);
        header.Children.Add(new TextBlock { Text = "BOSSES", Style = (Style)Application.Current.FindResource("PageTitle") });
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var intro = Ui.Body("Pick which bosses ring. Give the big ones their own sound so you know what's up before you look.");
        intro.Margin = new Thickness(0, 0, 0, 14);
        DockPanel.SetDock(intro, Dock.Top);
        root.Children.Add(intro);

        var head = Line(new[] { "BOSS", "TYPE", "PER DAY", "NEXT", "SOUND", "ALARM" }.Select(t => (UIElement)Ui.Label(t)).ToArray());
        head.Margin = new Thickness(0, 0, 0, 2);
        DockPanel.SetDock(head, Dock.Top);
        root.Children.Add(head);

        root.Children.Add(new ScrollViewer { Content = _rows, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 10, 18) });
        Content = root;
    }

    private static FrameworkElement Spacer() => new Border { Width = 8 };

    private static Grid Line(UIElement[] cells)
    {
        var grid = new Grid();
        foreach (var width in Columns)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        for (var i = 0; i < cells.Length; i++)
        {
            Grid.SetColumn(cells[i], i);
            if (cells[i] is FrameworkElement fe && i > 0)
                fe.Margin = new Thickness(8, fe.Margin.Top, 0, fe.Margin.Bottom);
            grid.Children.Add(cells[i]);
        }
        return grid;
    }

    private void SetAll(Func<string, bool> on)
    {
        _state.Settings.DisabledBosses = [.. _state.Bosses.Where(b => !on(b))];
        _state.Save();
    }

    public void Refresh()
    {
        _rows.Children.Clear();
        var settings = _state.Settings;
        var zone = TimeZoneInfo.Local;
        var now = DateTime.UtcNow;
        var today = TimeZoneInfo.ConvertTimeFromUtc(now, zone).Date;
        var upcoming = BossSchedule.Upcoming(_state.Schedule.Slots, now, TimeSpan.FromDays(1));
        var defaultName = _state.SoundName(settings.SoundFor([]));
        var options = new List<SoundOption> { new("", $"Default ({defaultName})", false) };
        options.AddRange(_state.SoundOptions);

        var bosses = _state.Bosses
            .Select(b => (Boss: b, Next: upcoming.FirstOrDefault(s => s.Slot.Bosses.Contains(b, StringComparer.OrdinalIgnoreCase))))
            .OrderBy(x => x.Next?.Utc ?? DateTime.MaxValue);

        foreach (var (boss, next) in bosses)
        {
            var on = settings.IsEnabled(boss);
            var major = !_state.IsRegular(boss);

            var name = new TextBlock
            {
                Text = boss,
                FontFamily = Ui.Font("Body"),
                FontWeight = FontWeights.SemiBold,
                FontSize = 16,
                Foreground = Ui.Brush(on ? "Ink" : "Mute"),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(12, 0, 0, 0),
            };
            var face = Ui.Portrait(boss, 40, dimmed: !on);
            face.Margin = new Thickness(10, 0, 0, 0);
            var who = new DockPanel { VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(face, Dock.Left);
            who.Children.Add(face);
            who.Children.Add(name);
            var type = Ui.Chip(major ? "MAJOR" : "REGULAR", major, enabled: true);
            type.HorizontalAlignment = HorizontalAlignment.Left;
            type.VerticalAlignment = VerticalAlignment.Center;

            var perDay = new TextBlock
            {
                Text = $"{BossSchedule.SpawnsPerDay(_state.Schedule.Slots, boss)}×",
                Style = (Style)Application.Current.FindResource("Time"),
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var nextText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            if (next is not null)
            {
                var local = next.ToLocal(zone);
                nextText.Inlines.Add(new System.Windows.Documents.Run(local.ToString("HH:mm"))
                {
                    FontFamily = Ui.Font("Numerals"),
                    FontWeight = FontWeights.ExtraBold,
                    FontSize = 20,
                    Foreground = Ui.Brush("Ink"),
                });
                if (local.Date != today)
                {
                    nextText.Inlines.Add(new System.Windows.Documents.Run("  tmrw")
                    {
                        FontFamily = Ui.Font("Body"),
                        FontSize = 12,
                        Foreground = Ui.Brush("Mute"),
                    });
                }
            }

            var sound = new ComboBox { ItemsSource = options, VerticalAlignment = VerticalAlignment.Center };
            System.Windows.Automation.AutomationProperties.SetName(sound, $"Sound for {boss}");
            sound.SelectedItem = settings.BossSounds.TryGetValue(boss, out var id) && settings.IsAvailable(id)
                ? options.First(o => o.Id == id)
                : options[0];
            sound.SelectionChanged += (_, _) =>
            {
                if (sound.SelectedItem is not SoundOption option)
                    return;
                if (option.Id.Length == 0)
                    settings.BossSounds.Remove(boss);
                else
                    settings.BossSounds[boss] = option.Id;
                _state.Save();
            };

            var alarm = Ui.Switch("", on, value =>
            {
                settings.DisabledBosses.RemoveAll(b => b.Equals(boss, StringComparison.OrdinalIgnoreCase));
                if (!value)
                    settings.DisabledBosses.Add(boss);
                _state.Save();
            });
            alarm.HorizontalAlignment = HorizontalAlignment.Left;
            System.Windows.Automation.AutomationProperties.SetName(alarm, $"Alarm for {boss}");

            var line = Line([who, type, perDay, nextText, sound, alarm]);
            line.MinHeight = 48;
            _rows.Children.Add(new Border
            {
                Background = Ui.Brush(on ? "Surface" : "Ground"),
                BorderBrush = Ui.Brush("Ground"),
                BorderThickness = new Thickness(0, 0, 0, 2),
                Padding = new Thickness(0, 4, 8, 4),
                Child = line,
            });
        }
    }
}
