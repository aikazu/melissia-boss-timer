using MelissiaBossTimer.Core;

namespace MelissiaBossTimer;

internal sealed record SoundOption(string Id, string Name, bool IsCustom)
{
    public override string ToString() => Name;
}

internal sealed record LeadOption(int Minutes)
{
    public static IReadOnlyList<LeadOption> All { get; } = [.. new[] { 0, 1, 2, 3, 5, 10, 15, 20, 30 }.Select(m => new LeadOption(m))];

    public override string ToString() => Minutes switch
    {
        0 => "At spawn",
        1 => "1 minute before",
        _ => $"{Minutes} minutes before",
    };
}

/// <summary>Settings, schedule and sound library shared by every page. Pages listen to <see cref="Changed"/> and redraw.</summary>
internal sealed class AppState
{
    public AppSettings Settings { get; } = AppSettings.Load(AppPaths.Settings);
    public SoundLibrary Sounds { get; } = new(AppPaths.Folder);
    public AlarmPlayer Player { get; } = new();
    public LoadedSchedule Schedule { get; set; } = ScheduleSource.LoadOffline();
    public string? LastError { get; set; }

    public event Action? Changed;

    public IReadOnlyList<string> Bosses => BossSchedule.AllBosses(Schedule.Slots);

    public bool IsRegular(string boss) => BossSchedule.IsRegular(Schedule.Slots, boss);

    public IReadOnlyList<SoundOption> SoundOptions =>
    [
        .. BuiltInSounds.All.Select(s => new SoundOption(s.Id, s.Name, false)),
        .. Settings.CustomSounds.Select(c => new SoundOption(c.Id, c.Name, true)),
    ];

    public string SoundName(string id) => Sounds.NameOf(id, Settings);

    public void Save()
    {
        try
        {
            Settings.Save(AppPaths.Settings);
            LastError = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LastError = $"Couldn't save settings: {ex.Message}";
        }
        Changed?.Invoke();
    }

    public void NotifyChanged() => Changed?.Invoke();

    public void PreviewOrStop(string soundId)
    {
        var path = Sounds.PathFor(soundId, Settings);
        if (Player.CurrentPath == path)
            Player.Stop();
        else
            Player.Preview(path, Settings.Volume);
        Changed?.Invoke();
    }

    public bool IsPreviewing(string soundId) =>
        Player.CurrentPath is { } current && current == Sounds.PathFor(soundId, Settings);
}
