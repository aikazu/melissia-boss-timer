using System.Windows;
using MelissiaBossTimer.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MelissiaBossTimer.Core;

namespace MelissiaBossTimer.Pages;

/// <summary>Fixtures of the next 24 hours grouped by part of day, with the alarm panel beside them.</summary>
internal sealed class SchedulePage : UserControl, IPage
{
    private readonly AppState _state;
    private readonly MainWindow _host;
    private readonly StackPanel _left = new();
    private readonly StackPanel _right = new() { Margin = new Thickness(14, 0, 0, 0) };
    private readonly TextBlock _nextAlarmTime = new() { Style = (Style)Application.Current.FindResource("Time"), FontSize = 40 };
    private readonly TextBlock _nextAlarmBosses = new() { Style = (Style)Application.Current.FindResource("BodyText"), Foreground = Ui.Brush("Ink"), FontWeight = FontWeights.SemiBold };
    private readonly ComboBox _lead = new() { ItemsSource = LeadOption.All };
    private readonly ComboBox _sound = new();
    private readonly Button _preview;
    private readonly Slider _volume = new() { Minimum = 0, Maximum = 100, SmallChange = 5, LargeChange = 10 };
    private readonly TextBlock _volumeText = new() { Style = (Style)Application.Current.FindResource("Time"), FontSize = 20, Width = 52, TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
    private readonly StackPanel _toggles = new();
    private bool _updating;

    public SchedulePage(AppState state, MainWindow host)
    {
        _state = state;
        _host = host;
        Focusable = false;

        var layout = new Grid { Margin = new Thickness(22, 18, 0, 0) };
        layout.ColumnDefinitions.Add(new ColumnDefinition());
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(286) });

        // Fixtures
        var fixtures = new DockPanel { Margin = new Thickness(0, 0, 18, 0) };
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
        var legend = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        legend.Children.Add(Ui.Chip("Major", major: true, enabled: true));
        legend.Children.Add(Ui.Chip("Regular", major: false, enabled: true));
        legend.Children.Add(Ui.Chip("Alarm off", major: false, enabled: false));
        DockPanel.SetDock(legend, Dock.Right);
        header.Children.Add(legend);
        header.Children.Add(new TextBlock { Text = "FIXTURES", Style = (Style)Application.Current.FindResource("PageTitle") });
        DockPanel.SetDock(header, Dock.Top);
        fixtures.Children.Add(header);

        var columns = new Grid();
        columns.ColumnDefinitions.Add(new ColumnDefinition());
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
        columns.ColumnDefinitions.Add(new ColumnDefinition());
        Grid.SetColumn(_right, 2);
        _right.Margin = new Thickness(0);
        columns.Children.Add(_left);
        columns.Children.Add(_right);
        fixtures.Children.Add(new ScrollViewer { Content = columns, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 10, 18) });
        layout.Children.Add(fixtures);

        // Alarm panel
        _preview = Ui.IconButton(Ui.Glyph.Play, "Preview sound", (_, _) => _state.PreviewOrStop(SelectedSoundId()));
        var panel = BuildPanel();
        Grid.SetColumn(panel, 1);
        layout.Children.Add(panel);

        Content = layout;
    }

    private FrameworkElement BuildPanel()
    {
        var stack = new StackPanel { Margin = new Thickness(18, 16, 18, 18) };
        stack.Children.Add(new TextBlock { Text = "ALARM", Style = (Style)Application.Current.FindResource("SectionTitle"), Margin = new Thickness(0, 0, 0, 12) });

        // Next alarm, cut out of a white jagged frame.
        var next = new Grid { Margin = new Thickness(0, 0, 0, 18) };
        next.Children.Add(new Shard
        {
            Points = [new(0, 0.06), new(1, 0), new(0.97, 1), new(0.02, 0.94)],
            Stroke = Ui.Brush("Ink"),
            StrokeThickness = 2,
            Fill = Ui.Brush("Ground"),
        });
        var nextText = new StackPanel { Margin = new Thickness(14, 8, 12, 10) };
        nextText.Children.Add(_nextAlarmTime);
        nextText.Children.Add(_nextAlarmBosses);
        next.Children.Add(nextText);
        stack.Children.Add(next);

        stack.Children.Add(Ui.Label("ALERT"));
        _lead.SelectionChanged += (_, _) =>
        {
            if (_updating || _lead.SelectedItem is not LeadOption lead)
                return;
            _state.Settings.LeadMinutes = lead.Minutes;
            _state.Save();
        };
        stack.Children.Add(_lead);

        stack.Children.Add(Spaced(Ui.Label("SOUND")));
        var soundRow = new DockPanel();
        DockPanel.SetDock(_preview, Dock.Right);
        _preview.Margin = new Thickness(8, 0, 0, 0);
        soundRow.Children.Add(_preview);
        soundRow.Children.Add(_sound);
        _sound.SelectionChanged += (_, _) =>
        {
            if (_updating || _sound.SelectedItem is not SoundOption option)
                return;
            _state.Settings.DefaultSoundId = option.Id;
            _state.Save();
        };
        stack.Children.Add(soundRow);

        stack.Children.Add(Spaced(Ui.Label("VOLUME")));
        var volumeRow = new DockPanel();
        DockPanel.SetDock(_volumeText, Dock.Right);
        volumeRow.Children.Add(_volumeText);
        volumeRow.Children.Add(_volume);
        System.Windows.Automation.AutomationProperties.SetName(_volume, "Alarm volume");
        _volume.ValueChanged += (_, e) =>
        {
            _volumeText.Text = $"{(int)e.NewValue}%";
            if (_updating)
                return;
            _state.Settings.Volume = (int)e.NewValue;
            _state.Player.SetVolume(_state.Settings.Volume);
        };
        _volume.LostMouseCapture += (_, _) => _state.Save();
        _volume.KeyUp += (_, _) => _state.Save();
        stack.Children.Add(volumeRow);

        stack.Children.Add(Spaced(Ui.Label("QUICK TOGGLES")));
        stack.Children.Add(_toggles);

        var test = Ui.Button("Test alarm", (_, _) => _host.TestAlarm(), primary: false, glyph: Ui.Glyph.Bell);
        test.HorizontalAlignment = HorizontalAlignment.Left;
        test.Margin = new Thickness(0, 14, 0, 0);
        stack.Children.Add(test);

        return new Border
        {
            Background = Ui.Brush("Surface"),
            Child = new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto },
        };
    }

    private static FrameworkElement Spaced(FrameworkElement e)
    {
        e.Margin = new Thickness(0, 14, 0, 6);
        return e;
    }

    private string SelectedSoundId() => (_sound.SelectedItem as SoundOption)?.Id ?? _state.Settings.DefaultSoundId;

    public void Refresh()
    {
        _updating = true;
        var settings = _state.Settings;
        _lead.SelectedItem = LeadOption.All.FirstOrDefault(l => l.Minutes == settings.LeadMinutes) ?? LeadOption.All[4];
        var options = _state.SoundOptions;
        _sound.ItemsSource = options;
        _sound.SelectedItem = options.FirstOrDefault(o => o.Id == settings.DefaultSoundId) ?? options[0];
        _volume.Value = settings.Volume;
        _volumeText.Text = $"{settings.Volume}%";
        _preview.Content = _state.IsPreviewing(SelectedSoundId()) ? Ui.Glyph.Stop : Ui.Glyph.Play;
        _preview.ToolTip = _state.IsPreviewing(SelectedSoundId()) ? "Stop preview" : "Preview sound";
        BuildToggles();
        _updating = false;

        BuildFixtures();
        UpdateNextAlarm();
    }

    private void BuildToggles()
    {
        _toggles.Children.Clear();
        var settings = _state.Settings;
        var majors = _state.Bosses.Where(b => !_state.IsRegular(b)).ToList();
        _toggles.Children.Add(Ui.Switch("All major bosses", majors.All(settings.IsEnabled), on =>
        {
            foreach (var boss in majors)
                SetEnabled(boss, on);
            _state.Save();
        }));
        foreach (var boss in _state.Bosses.Where(_state.IsRegular))
        {
            _toggles.Children.Add(Ui.Switch(boss, settings.IsEnabled(boss), on =>
            {
                SetEnabled(boss, on);
                _state.Save();
            }));
        }
    }

    private void SetEnabled(string boss, bool on)
    {
        var disabled = _state.Settings.DisabledBosses;
        disabled.RemoveAll(b => b.Equals(boss, StringComparison.OrdinalIgnoreCase));
        if (!on)
            disabled.Add(boss);
    }

    private void UpdateNextAlarm()
    {
        var settings = _state.Settings;
        var now = DateTime.UtcNow;
        var next = BossSchedule.Upcoming(_state.Schedule.Slots, now, TimeSpan.FromHours(24))
            .FirstOrDefault(s => s.Slot.Bosses.Any(settings.IsEnabled));
        if (next is null)
        {
            _nextAlarmTime.Text = "OFF";
            _nextAlarmBosses.Text = "No bosses are set to ring. Turn some on below or in Bosses.";
            return;
        }
        var at = next.Utc - TimeSpan.FromMinutes(settings.LeadMinutes);
        _nextAlarmTime.Text = TimeZoneInfo.ConvertTimeFromUtc(at < now ? now : at, TimeZoneInfo.Local).ToString("HH:mm");
        _nextAlarmBosses.Text = $"for {string.Join(", ", next.Slot.Bosses.Where(settings.IsEnabled))} at {next.ToLocal(TimeZoneInfo.Local):HH:mm}";
    }

    private void BuildFixtures()
    {
        _left.Children.Clear();
        _right.Children.Clear();
        var zone = TimeZoneInfo.Local;
        var now = DateTime.UtcNow;
        var today = TimeZoneInfo.ConvertTimeFromUtc(now, zone).Date;
        var settings = _state.Settings;
        var spawns = BossSchedule.Upcoming(_state.Schedule.Slots, now, TimeSpan.FromHours(24));
        var nextWanted = spawns.FirstOrDefault(s => s.Slot.Bosses.Any(settings.IsEnabled));

        int leftRows = 0, rightRows = 0;
        foreach (var part in BossSchedule.GroupByDayPart(spawns, zone))
        {
            var first = part.Spawns[0].ToLocal(zone);
            var last = part.Spawns[^1].ToLocal(zone);
            var range = part.Spawns.Count == 1 ? $"{first:HH:mm}" : $"{first:HH:mm}–{last:HH:mm}";
            if (part.LocalDate > today)
                range += " · TOMORROW";

            var card = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            card.Children.Add(Ui.Tab(part.Name, range));
            var rows = new StackPanel { Background = Ui.Brush("Surface"), Margin = new Thickness(0, 4, 0, 0) };
            foreach (var spawn in part.Spawns)
                rows.Children.Add(Row(spawn, spawn == nextWanted));
            card.Children.Add(rows);

            // Fill the shorter column so the two stay balanced.
            if (leftRows <= rightRows)
            {
                _left.Children.Add(card);
                leftRows += part.Spawns.Count + 2;
            }
            else
            {
                _right.Children.Add(card);
                rightRows += part.Spawns.Count + 2;
            }
        }
    }

    private FrameworkElement Row(Spawn spawn, bool isNext)
    {
        var settings = _state.Settings;
        var anyOn = spawn.Slot.Bosses.Any(settings.IsEnabled);
        var grid = new Grid { MinHeight = 36, Background = isNext ? Ui.Brush("Red") : Brushes.Transparent };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(76) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        var time = new TextBlock
        {
            Text = spawn.ToLocal(TimeZoneInfo.Local).ToString("HH:mm"),
            Style = (Style)Application.Current.FindResource("Time"),
            FontSize = 21,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 0, 0),
            Foreground = Ui.Brush(anyOn || isNext ? "Ink" : "Dim"),
        };
        if (isNext)
        {
            // NEXT sits under the time so the chips keep the row's full width for the boss names.
            var badge = new Grid { Margin = new Thickness(12, 2, 0, 6), HorizontalAlignment = HorizontalAlignment.Left };
            badge.Children.Add(new Shard { Points = [new(0.15, 0), new(1, 0), new(0.85, 1), new(0, 1)], Fill = Ui.Brush("Ground") });
            badge.Children.Add(new TextBlock { Text = "NEXT", FontFamily = Ui.Font("Display"), FontSize = 13, Foreground = Ui.Brush("Ink"), Margin = new Thickness(9, 1, 9, 2) });
            time.Margin = new Thickness(12, 6, 0, 0);
            grid.Children.Add(new StackPanel { VerticalAlignment = VerticalAlignment.Center, Children = { time, badge } });
        }
        else
        {
            grid.Children.Add(time);
        }

        var chips = new WrapPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 4, 8, 4) };
        foreach (var boss in spawn.Slot.Bosses)
            chips.Children.Add(Ui.Chip(boss, !_state.IsRegular(boss), settings.IsEnabled(boss), onRed: isNext));
        Grid.SetColumn(chips, 1);
        grid.Children.Add(chips);

        return new Border
        {
            BorderBrush = Ui.Brush("Ground"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = grid,
        };
    }
}
