namespace MelissiaBossTimer.Core;

/// <summary>Resolves sound ids to playable files and imports user sounds into the app folder.</summary>
public sealed class SoundLibrary(string appFolder)
{
    public static readonly string[] SupportedExtensions = [".wav", ".mp3"];
    public const long MaxImportBytes = 20 * 1024 * 1024;

    private string SoundsFolder => Path.Combine(appFolder, "sounds");

    public CustomSound Import(string sourcePath, AppSettings settings)
    {
        var ext = Path.GetExtension(sourcePath).ToLowerInvariant();
        if (!SupportedExtensions.Contains(ext))
            throw new InvalidDataException("Only .wav and .mp3 files can be used as alarm sounds.");
        var info = new FileInfo(sourcePath);
        if (!info.Exists)
            throw new FileNotFoundException("The sound file no longer exists.", sourcePath);
        if (info.Length > MaxImportBytes)
            throw new InvalidDataException("That file is over 20 MB. Alarm sounds should be short clips.");

        Directory.CreateDirectory(SoundsFolder);
        var id = "custom:" + Guid.NewGuid().ToString("N");
        var sound = new CustomSound
        {
            Id = id,
            Name = Path.GetFileNameWithoutExtension(sourcePath),
            FileName = id["custom:".Length..] + ext,
        };
        File.Copy(sourcePath, Path.Combine(SoundsFolder, sound.FileName));
        settings.CustomSounds.Add(sound);
        return sound;
    }

    public void Remove(string soundId, AppSettings settings)
    {
        var sound = settings.CustomSounds.FirstOrDefault(c => c.Id == soundId);
        if (sound is null)
            return;
        settings.CustomSounds.Remove(sound);
        foreach (var boss in settings.BossSounds.Where(p => p.Value == soundId).Select(p => p.Key).ToList())
            settings.BossSounds.Remove(boss);
        if (settings.DefaultSoundId == soundId)
            settings.DefaultSoundId = BuiltInSounds.DefaultId;
        try
        {
            File.Delete(Path.Combine(SoundsFolder, sound.FileName));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A file still held by the player is left behind; it's no longer referenced.
        }
    }

    /// <summary>Path of a playable file for the id. Built-in tones are rendered to disk on first use.</summary>
    public string PathFor(string soundId, AppSettings settings)
    {
        if (BuiltInSounds.IsBuiltIn(soundId))
        {
            var folder = Path.Combine(SoundsFolder, "builtin");
            var path = Path.Combine(folder, soundId["builtin:".Length..] + ".wav");
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(folder);
                File.WriteAllBytes(path, BuiltInSounds.RenderWav(soundId));
            }
            return path;
        }

        var custom = settings.CustomSounds.FirstOrDefault(c => c.Id == soundId);
        var customPath = custom is null ? null : Path.Combine(SoundsFolder, custom.FileName);
        return customPath is not null && File.Exists(customPath)
            ? customPath
            : PathFor(BuiltInSounds.DefaultId, settings);
    }

    public string NameOf(string soundId, AppSettings settings) =>
        BuiltInSounds.All.FirstOrDefault(s => s.Id == soundId)?.Name
        ?? settings.CustomSounds.FirstOrDefault(c => c.Id == soundId)?.Name
        ?? BuiltInSounds.All[0].Name;
}
