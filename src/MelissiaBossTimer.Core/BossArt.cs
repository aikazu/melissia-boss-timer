namespace MelissiaBossTimer.Core;

/// <summary>
/// Maps a boss name from the schedule to the key of its portrait (Assets/Bosses/{key}.png).
/// Variants share art (Stormbringer Karanda, Thundercloud Ancient Kutum); unknown names get null,
/// since the online schedule can introduce bosses the app has no art for.
/// </summary>
public static class BossArt
{
    public static readonly IReadOnlyList<string> Keys =
    [
        "kzarka", "karanda", "kutum", "garmoth", "vell", "quint", "muraka", "bulgasal",
        "uturi", "sangoon", "golden-pig-king", "targargo", "katzvariak", "giant-mudster", "red-nose",
    ];

    private static readonly (string Fragment, string Key)[] Aliases =
    [
        ("kutum", "kutum"),
        ("karanda", "karanda"),
        ("kvariak", "katzvariak"),
        ("katsvariak", "katzvariak"),
        ("targarg", "targargo"),
        ("golden pig", "golden-pig-king"),
        ("mudster", "giant-mudster"),
    ];

    public static string? KeyFor(string boss)
    {
        var name = boss.Trim().ToLowerInvariant();
        if (name.Length == 0)
            return null;
        foreach (var (fragment, key) in Aliases)
        {
            if (name.Contains(fragment, StringComparison.Ordinal))
                return key;
        }
        var slug = string.Join('-', name.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return Keys.Contains(slug) ? slug : null;
    }
}
