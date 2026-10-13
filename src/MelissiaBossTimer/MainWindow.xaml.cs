using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MelissiaBossTimer.Core;
using MelissiaBossTimer.Pages;
using Microsoft.Win32;

namespace MelissiaBossTimer;

public partial class MainWindow : Window
{
    private static readonly TimeSpan ListWindow = TimeSpan.FromHours(24);
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(12);

    private readonly AppState _state = new();
    private readonly AlarmPlanner _planner = new();
    private readonly DispatcherTimer _tick = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly TrayIcon _tray = new();
    private readonly Dictionary<string, FrameworkElement> _pages = [];

    private AlarmWindow? _alarmWindow;
    private Alarm? _firing;
    private DateTime? _bannerSpawn;
    private DateTime? _firstListedSpawn;
    private DateTime _listedLocalDate;
    private DateTime _lastRefreshAttempt = DateTime.MinValue;
    private bool _exiting;
    private bool _trayHintShown;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => DarkTitleBar.Apply(this);

        _tray.OpenRequested += ShowFromTray;
        _tray.TestRequested += TestAlarm;
        _tray.ExitRequested += ExitApp;
        _state.Changed += OnStateChanged;
        _state.Player.Stopped += _state.NotifyChanged;
        _state.Player.Failed += (path, error) =>
        {
            _state.LastError = $"Couldn't play {Path.GetFileName(path)} ({error}).";
            _state.NotifyChanged();
        };

        ShowPage("schedule");
        RefreshAll();

        _tick.Tick += (_, _) => OnTick();
        _tick.Start();
        SystemEvents.TimeChanged += OnSystemTimeChanged;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private const double FaceSize = 184;

    internal AppState State => _state;

    public void ShowFromTray()
    {
        Show();
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        Activate();
    }

    internal void TestAlarm()
    {
        var now = DateTime.UtcNow;
        var spawn = BossSchedule.Upcoming(_state.Schedule.Slots, now, ListWindow).FirstOrDefault()
            ?? new Spawn(now.AddMinutes(5), new BossSlot(TimeOnly.FromDateTime(now), ["Test"]));
        RaiseAlarm(new Alarm(spawn, spawn.Slot.Bosses));
    }

    internal async Task RefreshScheduleAsync()
    {
        _lastRefreshAttempt = DateTime.UtcNow;
        RefreshLink.IsEnabled = false;
        StatusLine.Text = "Checking the wiki for schedule changes…";
        try
        {
            _state.Schedule = await ScheduleSource.FetchOnlineAsync();
            _state.LastError = null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or FormatException)
        {
            _state.LastError = $"Couldn't update the schedule ({ex.Message}). Using the {_state.Schedule.Description}.";
        }
        finally
        {
            RefreshLink.IsEnabled = true;
        }
        _state.NotifyChanged();
    }

    private void OnTick()
    {
        var now = DateTime.UtcNow;
        var lead = TimeSpan.FromMinutes(_state.Settings.LeadMinutes);
        foreach (var alarm in _planner.Due(_state.Schedule.Slots, now, lead, _state.Settings.IsEnabled))
            RaiseAlarm(alarm);

        var localDate = TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.Local).Date;
        // Redraw when a spawn passes, or at local midnight so Today/Tomorrow labels stay right.
        if (_firstListedSpawn is { } first && first <= now || localDate != _listedLocalDate)
            RefreshAll();
        else
            UpdateClockAndBanner();

        if (now - _lastRefreshAttempt > RefreshInterval)
            _ = RefreshScheduleAsync();
    }

    private void OnStateChanged() => RefreshAll();

    private void RefreshAll()
    {
        var now = DateTime.UtcNow;
        var spawns = BossSchedule.Upcoming(_state.Schedule.Slots, now, ListWindow);
        _firstListedSpawn = spawns.Count > 0 ? spawns[0].Utc : null;
        _listedLocalDate = TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.Local).Date;

        foreach (var page in _pages.Values.OfType<IPage>())
            page.Refresh();
        UpdateClockAndBanner();
        UpdateStatusLine();
    }

    private void UpdateClockAndBanner()
    {
        var zone = TimeZoneInfo.Local;
        var nowUtc = DateTime.UtcNow;
        var local = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, zone);
        var offset = zone.GetUtcOffset(nowUtc);
        ClockText.Text = local.ToString("HH:mm:ss");
        ClockMeta.Text = $"{local:ddd d MMM}  ·  UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset:hh\\:mm}".ToUpperInvariant();

        var settings = _state.Settings;
        var upcoming = BossSchedule.Upcoming(_state.Schedule.Slots, nowUtc, ListWindow);
        var next = upcoming.FirstOrDefault(s => s.Slot.Bosses.Any(settings.IsEnabled));
        var anyEnabled = next is not null;
        next ??= upcoming.FirstOrDefault();
        if (next is null)
            return;

        var bosses = anyEnabled ? next.Slot.Bosses.Where(settings.IsEnabled).ToList() : [.. next.Slot.Bosses];
        var spawnLocal = next.ToLocal(zone);
        var left = next.Utc - nowUtc;
        var alarmAt = next.Utc - TimeSpan.FromMinutes(settings.LeadMinutes);
        var inAlarmWindow = anyEnabled && nowUtc >= alarmAt;

        var names = string.Join(" · ", bosses);
        if (_bannerSpawn != next.Utc || BossNames.Text != names)
        {
            _bannerSpawn = next.Utc;
            BossNames.Text = names;
            var lead = BossImages.Lead(bosses, _state.IsRegular);
            BannerFace.Content = lead is null ? null : Pages.Ui.Cutout(lead, FaceSize);
            // The status line may run over the face's column; the names and times keep clear of where the art starts
            // (column = size minus the 40px tucked under the countdown, less the content's 40px right margin and the slash).
            var gap = lead is null ? 0 : FaceSize - 40 - 40 - FaceSize * 0.13 + 8;
            NamesBox.Margin = new Thickness(0, 4, gap, 2);
            TimeRow.Margin = new Thickness(0, 0, gap, 0);
            SnapBanner();
        }

        SpawnTime.Text = spawnLocal.ToString("HH:mm");
        var dayWord = spawnLocal.Date == local.Date ? "TODAY" : spawnLocal.Date == local.Date.AddDays(1) ? "TOMORROW" : spawnLocal.ToString("ddd d MMM").ToUpperInvariant();
        DateMeta.Text = $"{dayWord} · LOCAL TIME";
        var then = upcoming.SkipWhile(s => s != next).Skip(1).FirstOrDefault(s => s.Slot.Bosses.Any(settings.IsEnabled));
        ThenText.Text = then is null ? "" : $"then {then.ToLocal(zone):HH:mm} {string.Join(", ", then.Slot.Bosses.Where(settings.IsEnabled))}";

        var sound = settings.Sound ? _state.SoundName(settings.SoundFor(bosses)).ToUpperInvariant() : "SILENT";
        if (!anyEnabled)
            StatusText.Text = "ALL ALARMS OFF";
        else if (_firing is not null && _firing.Spawn.Utc == next.Utc)
            StatusText.Text = "ALARM NOW";
        else if (settings.LeadMinutes == 0)
            StatusText.Text = $"ALARM AT SPAWN · {sound}";
        else
            StatusText.Text = $"ALARM {TimeZoneInfo.ConvertTimeFromUtc(alarmAt, zone):HH:mm} · {sound}";
        var firing = _firing is not null && _firing.Spawn.Utc == next.Utc;
        // An alarm nobody will hear must not look armed: silent or all-off gets the dashed alarm-off grammar in red.
        var muted = !firing && (!anyEnabled || !settings.Sound);
        StatusPlate.Fill = (Brush)FindResource(firing ? "Ink" : "Ground");
        StatusPlate.Stroke = muted ? (Brush)FindResource("Ink") : null;
        StatusPlate.StrokeThickness = 1.5;
        StatusPlate.StrokeDashArray = muted ? [3, 2] : null;
        StatusText.Foreground = (Brush)FindResource(firing || muted ? "Red" : "Ink");

        CountdownText.Text = Countdown(left);
        // h:mm:ss needs a smaller cut to stay inside the frame.
        CountdownText.FontSize = left >= TimeSpan.FromHours(1) ? 74 : 96;
        CountFrame.Fill = (Brush)FindResource(inAlarmWindow ? "Red" : "Ground");
        CountdownCaption.Foreground = (Brush)FindResource(inAlarmWindow ? "Ink" : "Mute");

        _tray.SetTooltip(anyEnabled ? $"Next: {string.Join(", ", bosses)} {spawnLocal:HH:mm}" : "Melissia Boss Timer: all alarms off");
    }

    private void SnapBanner()
    {
        if (!SystemParameters.ClientAreaAnimation || !IsVisible)
            return;
        var ease = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 7 };
        BannerSlide.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(-90, 0, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease });
    }

    internal static string Countdown(TimeSpan t)
    {
        if (t < TimeSpan.Zero)
            t = TimeSpan.Zero;
        return t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes:00}:{t.Seconds:00}";
    }

    private void UpdateStatusLine()
    {
        var zone = TimeZoneInfo.Local;
        StatusLine.Text = _state.LastError
            ?? $"PC time zone {zone.DisplayName}  ·  schedule: {_state.Schedule.Description}  ·  the wiki lists times in GMT";
        StatusLine.Foreground = (Brush)FindResource(_state.LastError is null ? "Mute" : "Ink");
    }

    private void RaiseAlarm(Alarm alarm)
    {
        _firing = alarm;
        var settings = _state.Settings;
        var local = alarm.Spawn.ToLocal(TimeZoneInfo.Local);
        var left = alarm.Spawn.Utc - DateTime.UtcNow;
        _tray.Notify(left > TimeSpan.Zero ? $"Boss spawn in {Countdown(left)}" : "Boss spawning now",
            $"{string.Join(", ", alarm.Bosses)} at {local:HH:mm}", warning: true);

        if (settings.Sound)
            _state.Player.PlayAlarm(_state.Sounds.PathFor(settings.SoundFor(alarm.Bosses), settings), settings.Volume);

        if (_alarmWindow is null || !_alarmWindow.IsLoaded)
        {
            _alarmWindow = new AlarmWindow();
            _alarmWindow.Closed += (_, _) =>
            {
                _state.Player.Stop();
                _firing = null;
                _alarmWindow = null;
                UpdateClockAndBanner();
            };
        }
        _alarmWindow.ShowAlarm(alarm, settings.Sound ? _state.SoundName(settings.SoundFor(alarm.Bosses)) : "Sound off", _state.IsRegular);
        UpdateClockAndBanner();
    }

    private void OnNavChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string key } && IsInitialized)
            ShowPage(key);
    }

    private void ShowPage(string key)
    {
        if (PageHost is null)
            return;
        if (!_pages.TryGetValue(key, out var page))
        {
            page = key switch
            {
                "bosses" => new BossesPage(_state),
                "sounds" => new SoundsPage(_state),
                "settings" => new SettingsPage(_state, this),
                _ => new SchedulePage(_state, this),
            };
            _pages[key] = page;
        }
        PageHost.Content = page;
        if (page is IPage p)
            p.Refresh();
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e) => await RefreshScheduleAsync();

    private void OnSystemTimeChanged(object? sender, EventArgs e) => Dispatcher.BeginInvoke(() =>
    {
        // Time zone or clock changed while running: drop the cached zone so local times follow the PC.
        TimeZoneInfo.ClearCachedData();
        _bannerSpawn = null;
        RefreshAll();
    });

    private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
            OnSystemTimeChanged(sender, e);
    }

    private void ExitApp()
    {
        _exiting = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_exiting)
        {
            // Closing the window keeps alarms running; Exit is in the tray menu.
            e.Cancel = true;
            Hide();
            if (!_trayHintShown)
            {
                _trayHintShown = true;
                _tray.Notify("Melissia Boss Timer", "Still running in the tray. Right-click the icon to exit.", warning: false);
            }
            return;
        }
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        SystemEvents.TimeChanged -= OnSystemTimeChanged;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        _tick.Stop();
        _state.Player.Stop();
        _alarmWindow?.Close();
        _tray.Dispose();
        base.OnClosed(e);
        Application.Current.Shutdown();
    }
}

internal interface IPage
{
    void Refresh();
}
