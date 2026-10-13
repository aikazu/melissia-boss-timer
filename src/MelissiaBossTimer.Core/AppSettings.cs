using System.Text.Json;

namespace MelissiaBossTimer.Core;

public sealed class CustomSound
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    /// <summary>File name inside the app's sounds folder; imports are copied there so moving the original doesn't break the alarm.</summary>
    public string FileName { get; set; } = "";
}

public sealed class AppSettings
{
    public int LeadMinutes { get; set; } = 5;
    public bool Sound { get; set; } = true;
    public int Volume { get; set; } = 80;
    public string DefaultSoundId { get; set; } = BuiltInSounds.DefaultId;

    /// <summary>Stored as the disabled set so bosses added to the schedule later alarm by default.</summary>
    public List<string> DisabledBosses { get; set; } = [];

    /// <summary>Boss name to sound id, for bosses that override the default sound.</summary>
    public Dictionary<string, string> BossSounds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<CustomSound> CustomSounds { get; set; } = [];

    public bool IsEnabled(string boss) => !DisabledBosses.Contains(boss, StringComparer.OrdinalIgnoreCase);

    public bool IsAvailable(string soundId) => BuiltInSounds.IsBuiltIn(soundId) || CustomSounds.Any(c => c.Id == soundId);

    /// <summary>
    /// The sound for an alarm: the first boss (in the wiki's listing order) with its own sound wins,
    /// otherwise the default. A sound whose custom file was removed falls back rather than going silent.
    /// </summary>
    public string SoundFor(IEnumerable<string> bosses)
    {
        foreach (var boss in bosses)
        {
            if (BossSounds.TryGetValue(boss, out var id) && IsAvailable(id))
                return id;
        }
        return IsAvailable(DefaultSoundId) ? DefaultSoundId : BuiltInSounds.DefaultId;
    }

    public static AppSettings Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings();
                // JSON deserialization drops the case-insensitive comparer.
                settings.BossSounds = new Dictionary<string, string>(settings.BossSounds, StringComparer.OrdinalIgnoreCase);
                settings.Volume = Math.Clamp(settings.Volume, 0, 100);
                return settings;
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Corrupt or unreadable settings fall back to defaults rather than blocking alarms.
        }
        return new AppSettings();
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
