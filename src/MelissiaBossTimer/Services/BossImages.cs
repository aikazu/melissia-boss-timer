using System.Windows.Media.Imaging;
using MelissiaBossTimer.Core;

namespace MelissiaBossTimer;

/// <summary>Loads the bundled boss portraits, decoded once per display size.</summary>
internal static class BossImages
{
    private static readonly Dictionary<(string Key, int Width), BitmapImage> Cache = [];

    /// <summary>The portrait for a boss at about <paramref name="pixels"/> wide, or null when there is no art.</summary>
    public static BitmapImage? For(string boss, double pixels)
    {
        if (BossArt.KeyFor(boss) is not { } key)
            return null;
        // Decode at 2x the layout size so it stays sharp at 200% scaling, in coarse steps to share the cache.
        var width = Math.Clamp((int)Math.Ceiling(pixels * 2 / 64) * 64, 64, 512);
        if (Cache.TryGetValue((key, width), out var cached))
            return cached;

        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri($"pack://application:,,,/Assets/Bosses/{key}.png");
        image.DecodePixelWidth = width;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        Cache[(key, width)] = image;
        return image;
    }

    /// <summary>The boss whose face fronts a group: a major with art first, then any with art.</summary>
    public static string? Lead(IEnumerable<string> bosses, Func<string, bool> isRegular)
    {
        var withArt = bosses.Where(b => BossArt.KeyFor(b) is not null).ToList();
        return withArt.FirstOrDefault(b => !isRegular(b)) ?? withArt.FirstOrDefault();
    }
}
