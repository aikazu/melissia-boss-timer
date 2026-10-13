namespace MelissiaBossTimer.Core;

/// <summary>A daily spawn slot. <see cref="UtcTime"/> is the site's "Time (GMT)" column.</summary>
public sealed record BossSlot(TimeOnly UtcTime, IReadOnlyList<string> Bosses);

/// <summary>One concrete spawn on a specific UTC date.</summary>
public sealed record Spawn(DateTime Utc, BossSlot Slot)
{
    public DateTime ToLocal(TimeZoneInfo zone) => TimeZoneInfo.ConvertTimeFromUtc(Utc, zone);
}

public static class BossSchedule
{
    public const string SourceUrl = "https://melissiagamesinfohub.vercel.app/en/wiki/boss-schedule";

    /// <summary>Snapshot of the site's spawn table (GMT), taken 2026-10-14. Used when the site can't be reached.</summary>
    public static IReadOnlyList<BossSlot> BuiltIn { get; } =
    [
        Slot(2, 0, "Bulgasal"),
        Slot(2, 15, "Uturi"),
        Slot(2, 30, "Golden Pig King"),
        Slot(2, 45, "Sangoon"),
        Slot(6, 0, "Targargo", "Bulgasal"),
        Slot(6, 15, "Uturi"),
        Slot(6, 30, "Katzvariak", "Golden Pig King"),
        Slot(6, 45, "Sangoon"),
        Slot(7, 0, "Giant Mudster", "Red Nose"),
        Slot(9, 0, "Stormbringer Karanda"),
        Slot(10, 0, "Vell", "Bulgasal"),
        Slot(10, 15, "Uturi"),
        Slot(10, 30, "Golden Pig King"),
        Slot(10, 45, "Sangoon"),
        Slot(11, 0, "Kzarka"),
        Slot(12, 30, "Quint", "Muraka"),
        Slot(13, 0, "Ancient Kutum"),
        Slot(14, 0, "Targargo", "Bulgasal"),
        Slot(14, 15, "Uturi"),
        Slot(14, 30, "Golden Pig King"),
        Slot(14, 45, "Sangoon"),
        Slot(15, 0, "Karanda"),
        Slot(16, 0, "Garmoth"),
        Slot(18, 0, "Bulgasal"),
        Slot(18, 15, "Uturi"),
        Slot(18, 30, "Giant Mudster", "Red Nose", "Golden Pig King"),
        Slot(18, 45, "Sangoon"),
        Slot(20, 30, "Thundercloud Ancient Kutum"),
        Slot(22, 0, "Kzarka", "Bulgasal"),
        Slot(22, 15, "Uturi"),
        Slot(22, 30, "Golden Pig King"),
        Slot(22, 45, "Sangoon"),
    ];

    private static BossSlot Slot(int hour, int minute, params string[] bosses) => new(new TimeOnly(hour, minute), bosses);

    /// <summary>
    /// Spawns with <c>fromUtc &lt;= spawn &lt; fromUtc + window</c>, ordered by time.
    /// Days are walked in UTC because the table is UTC; local dates are only applied when displaying.
    /// </summary>
    public static IReadOnlyList<Spawn> Upcoming(IReadOnlyList<BossSlot> slots, DateTime fromUtc, TimeSpan window)
    {
        if (fromUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Expected a UTC DateTime.", nameof(fromUtc));

        var end = fromUtc + window;
        var result = new List<Spawn>();
        for (var day = fromUtc.Date; day <= end.Date; day = day.AddDays(1))
        {
            foreach (var slot in slots)
            {
                var at = DateTime.SpecifyKind(day + slot.UtcTime.ToTimeSpan(), DateTimeKind.Utc);
                if (at >= fromUtc && at < end)
                    result.Add(new Spawn(at, slot));
            }
        }
        result.Sort((a, b) => a.Utc.CompareTo(b.Utc));
        return result;
    }

    public static IReadOnlyList<string> AllBosses(IReadOnlyList<BossSlot> slots) =>
        slots.SelectMany(s => s.Bosses).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToList();

    public static int SpawnsPerDay(IReadOnlyList<BossSlot> slots, string boss) =>
        slots.Count(s => s.Bosses.Contains(boss, StringComparer.OrdinalIgnoreCase));

    /// <summary>Regulars are the field bosses on the 4-hour cycle; derived from the data so a schedule change reclassifies them.</summary>
    public static bool IsRegular(IReadOnlyList<BossSlot> slots, string boss) => SpawnsPerDay(slots, boss) >= 4;

    /// <summary>Groups spawns (already in time order) into consecutive local parts of the day.</summary>
    public static IReadOnlyList<DayPart> GroupByDayPart(IReadOnlyList<Spawn> spawns, TimeZoneInfo zone)
    {
        var parts = new List<DayPart>();
        foreach (var spawn in spawns)
        {
            var local = spawn.ToLocal(zone);
            var name = DayPartName(local.Hour);
            if (parts.Count > 0 && parts[^1].Name == name && parts[^1].LocalDate == local.Date)
                parts[^1].Spawns.Add(spawn);
            else
                parts.Add(new DayPart(name, local.Date, [spawn]));
        }
        return parts;
    }

    private static string DayPartName(int hour) => hour switch
    {
        < 5 => "Night",
        < 8 => "Dawn",
        < 12 => "Morning",
        < 18 => "Afternoon",
        _ => "Evening",
    };
}

public sealed record DayPart(string Name, DateTime LocalDate, List<Spawn> Spawns);
