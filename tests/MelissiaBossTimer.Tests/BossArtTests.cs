using MelissiaBossTimer.Core;

namespace MelissiaBossTimer.Tests;

public class BossArtTests
{
    [Fact]
    public void Every_built_in_boss_has_a_portrait()
    {
        var missing = BossSchedule.AllBosses(BossSchedule.BuiltIn).Where(b => BossArt.KeyFor(b) is null).ToList();
        Assert.Empty(missing);
    }

    [Theory]
    [InlineData("Thundercloud Ancient Kutum", "kutum")]
    [InlineData("Ancient Kutum", "kutum")]
    [InlineData("Stormbringer Karanda", "karanda")]
    [InlineData("golden pig king", "golden-pig-king")]
    [InlineData("Kvariak", "katzvariak")]
    [InlineData("  Kzarka ", "kzarka")]
    public void Names_and_variants_map_to_shared_art(string boss, string key) => Assert.Equal(key, BossArt.KeyFor(boss));

    [Theory]
    [InlineData("Nouver")]
    [InlineData("")]
    public void Unknown_bosses_have_no_art(string boss) => Assert.Null(BossArt.KeyFor(boss));

    [Fact]
    public void Every_key_has_a_png_in_the_app()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "MelissiaBossTimer")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        var assets = Path.Combine(dir.FullName, "src", "MelissiaBossTimer", "Assets", "Bosses");
        var missing = BossArt.Keys.Where(k => !File.Exists(Path.Combine(assets, k + ".png"))).ToList();
        Assert.Empty(missing);
    }
}
