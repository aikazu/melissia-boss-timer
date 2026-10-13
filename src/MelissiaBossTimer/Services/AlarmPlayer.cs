using System.Windows.Media;
using System.Windows.Threading;

namespace MelissiaBossTimer;

/// <summary>
/// Plays alarm and preview sounds (WAV and MP3) at the app's own volume.
/// Sound is the primary alarm channel: Windows hides notifications over a fullscreen game, but audio still plays.
/// </summary>
internal sealed class AlarmPlayer
{
    private static readonly TimeSpan AlarmCap = TimeSpan.FromSeconds(90);

    private readonly MediaPlayer _player = new();
    private readonly DispatcherTimer _autoStop = new() { Interval = AlarmCap };
    private bool _loop;

    /// <summary>Raised when a file can't be played; the argument is the failing path.</summary>
    public event Action<string, string>? Failed;

    /// <summary>Raised when playback ends on its own (a preview finishing).</summary>
    public event Action? Stopped;

    public string? CurrentPath { get; private set; }
    public bool IsPlaying => CurrentPath is not null;

    public AlarmPlayer()
    {
        _autoStop.Tick += (_, _) => Stop();
        _player.MediaEnded += (_, _) =>
        {
            if (_loop)
            {
                _player.Position = TimeSpan.Zero;
                _player.Play();
            }
            else
            {
                Stop();
            }
        };
        _player.MediaFailed += (_, e) =>
        {
            var path = CurrentPath ?? "";
            Stop();
            Failed?.Invoke(path, e.ErrorException?.Message ?? "unknown error");
        };
    }

    public void PlayAlarm(string path, int volume) => Play(path, volume, loop: true);

    public void Preview(string path, int volume) => Play(path, volume, loop: false);

    public void SetVolume(int volume) => _player.Volume = Math.Clamp(volume, 0, 100) / 100.0;

    public void Stop()
    {
        var wasPlaying = IsPlaying;
        _autoStop.Stop();
        _player.Stop();
        _player.Close();
        CurrentPath = null;
        if (wasPlaying)
            Stopped?.Invoke();
    }

    private void Play(string path, int volume, bool loop)
    {
        Stop();
        _loop = loop;
        CurrentPath = path;
        SetVolume(volume);
        _player.Open(new Uri(path));
        _player.Play();
        if (loop)
            _autoStop.Start();
    }
}
