using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MelissiaBossTimer.Core;

namespace MelissiaBossTimer;

/// <summary>Topmost alarm popup drawn as a calling card. Closing it silences the alarm.</summary>
public partial class AlarmWindow : Window
{
    private readonly DispatcherTimer _tick = new() { Interval = TimeSpan.FromSeconds(1) };
    private Alarm? _alarm;

    public AlarmWindow()
    {
        InitializeComponent();
        _tick.Tick += (_, _) => UpdateLeft();
        MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
        Closed += (_, _) => _tick.Stop();
    }

    internal void ShowAlarm(Alarm alarm, string soundName, Func<string, bool> isRegular)
    {
        _alarm = alarm;
        Bosses.Text = string.Join(" · ", alarm.Bosses);
        var lead = BossImages.Lead(alarm.Bosses, isRegular);
        Face.Content = lead is null ? null : Pages.Ui.Portrait(lead, 180);
        SpawnTime.Text = alarm.Spawn.ToLocal(TimeZoneInfo.Local).ToString("HH:mm");
        SoundText.Text = soundName.ToUpperInvariant();
        UpdateLeft();
        _tick.Start();

        if (!IsVisible)
        {
            // Top-centre of the primary work area, above where game HUDs usually sit.
            var area = SystemParameters.WorkArea;
            Left = area.Left + (area.Width - Width) / 2;
            Top = area.Top + 48;
            Show();
        }
        Activate();
        SnapIn();
    }

    private void UpdateLeft()
    {
        if (_alarm is null)
            return;
        var left = _alarm.Spawn.Utc - DateTime.UtcNow;
        LeftText.Text = left > TimeSpan.Zero ? $"SPAWNS IN {MainWindow.Countdown(left)}" : "SPAWNING NOW";
    }

    private void SnapIn()
    {
        if (!SystemParameters.ClientAreaAnimation)
            return;
        var ease = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 7 };
        var duration = TimeSpan.FromMilliseconds(200);
        CardSlide.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, new DoubleAnimation(-140, 0, duration) { EasingFunction = ease });
        CardSkew.BeginAnimation(System.Windows.Media.SkewTransform.AngleXProperty, new DoubleAnimation(-14, 0, duration) { EasingFunction = ease });
    }

    private void OnDismiss(object sender, RoutedEventArgs e) => Close();
}
