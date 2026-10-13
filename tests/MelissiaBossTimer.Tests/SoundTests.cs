using MelissiaBossTimer.Core;

namespace MelissiaBossTimer.Tests;

public sealed class SoundTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "mbt-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, recursive: true);
    }

    [Theory]
    [MemberData(nameof(BuiltInIds))]
    public void Built_in_sounds_render_valid_unclipped_wav(string id)
    {
        var wav = BuiltInSounds.RenderWav(id);

        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(wav, 0, 4));
        Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(wav, 8, 4));
        Assert.Equal(wav.Length - 8, BitConverter.ToInt32(wav, 4));
        var peak = Enumerable.Range(0, (wav.Length - 44) / 2).Max(i => Math.Abs((int)BitConverter.ToInt16(wav, 44 + i * 2)));
        Assert.InRange(peak, short.MaxValue * 0.8, short.MaxValue * 0.9);
    }

    public static TheoryData<string> BuiltInIds() => new(BuiltInSounds.All.Select(s => s.Id));

    [Fact]
    public void Boss_sound_overrides_default_in_listing_order()
    {
        var settings = new AppSettings { DefaultSoundId = "builtin:chime" };
        settings.BossSounds["Kzarka"] = "builtin:war-horn";

        Assert.Equal("builtin:war-horn", settings.SoundFor(["Kzarka", "Bulgasal"]));
        Assert.Equal("builtin:war-horn", settings.SoundFor(["Bulgasal", "Kzarka"]));
        Assert.Equal("builtin:chime", settings.SoundFor(["Uturi"]));
    }

    [Fact]
    public void Import_copies_file_and_remove_falls_back_everywhere()
    {
        var source = Path.Combine(_folder, "my horn.mp3");
        Directory.CreateDirectory(_folder);
        File.WriteAllBytes(source, [1, 2, 3]);
        var settings = new AppSettings();
        var library = new SoundLibrary(Path.Combine(_folder, "app"));

        var sound = library.Import(source, settings);
        settings.DefaultSoundId = sound.Id;
        settings.BossSounds["Garmoth"] = sound.Id;
        File.Delete(source);

        Assert.Equal("my horn", sound.Name);
        Assert.True(File.Exists(library.PathFor(sound.Id, settings)));
        Assert.Equal(sound.Id, settings.SoundFor(["Garmoth"]));

        library.Remove(sound.Id, settings);

        Assert.Empty(settings.CustomSounds);
        Assert.Empty(settings.BossSounds);
        Assert.Equal(BuiltInSounds.DefaultId, settings.DefaultSoundId);
    }

    [Fact]
    public void Missing_custom_file_falls_back_to_built_in_path()
    {
        var settings = new AppSettings();
        settings.CustomSounds.Add(new CustomSound { Id = "custom:gone", Name = "Gone", FileName = "gone.wav" });
        var library = new SoundLibrary(_folder);

        var path = library.PathFor("custom:gone", settings);

        Assert.EndsWith("beacon.wav", path);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void Rejects_unsupported_files()
    {
        Directory.CreateDirectory(_folder);
        var source = Path.Combine(_folder, "notes.txt");
        File.WriteAllText(source, "x");

        Assert.Throws<InvalidDataException>(() => new SoundLibrary(_folder).Import(source, new AppSettings()));
    }

    [Fact]
    public void Settings_round_trip_keeps_case_insensitive_boss_sounds()
    {
        var path = Path.Combine(_folder, "settings.json");
        var settings = new AppSettings { Volume = 55 };
        settings.BossSounds["Kzarka"] = "builtin:siren";
        settings.Save(path);

        var loaded = AppSettings.Load(path);

        Assert.Equal(55, loaded.Volume);
        Assert.Equal("builtin:siren", loaded.SoundFor(["KZARKA"]));
    }

    [Fact]
    public void Old_settings_file_without_sound_fields_loads_with_defaults()
    {
        Directory.CreateDirectory(_folder);
        var path = Path.Combine(_folder, "settings.json");
        File.WriteAllText(path, """{ "LeadMinutes": 7, "Sound": true, "DisabledBosses": ["Uturi"] }""");

        var loaded = AppSettings.Load(path);

        Assert.Equal(7, loaded.LeadMinutes);
        Assert.Equal(80, loaded.Volume);
        Assert.Equal(BuiltInSounds.DefaultId, loaded.DefaultSoundId);
        Assert.False(loaded.IsEnabled("uturi"));
    }
}
