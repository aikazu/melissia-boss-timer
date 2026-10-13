using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MelissiaBossTimer.Core;

public static partial class ScheduleParser
{
    private const int MinimumRows = 20;

    /// <summary>Parses the spawn table from the wiki page HTML. Throws <see cref="FormatException"/> if the layout is not recognised.</summary>
    public static IReadOnlyList<BossSlot> ParseHtml(string html)
    {
        foreach (Match table in TableRegex().Matches(html))
        {
            var rows = RowRegex().Matches(table.Value).Select(r => Cells(r.Value)).Where(c => c.Count > 0).ToList();
            if (rows.Count == 0)
                continue;

            var header = rows[0];
            var timeColumn = header.FindIndex(h => h.Equals("Time (GMT)", StringComparison.OrdinalIgnoreCase));
            if (timeColumn < 0)
                continue;

            var bossColumns = Enumerable.Range(0, header.Count)
                .Where(i => header[i].StartsWith("Boss", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (bossColumns.Count == 0)
                throw new FormatException("Spawn table has no Boss columns.");

            var slots = new List<BossSlot>();
            foreach (var row in rows.Skip(1))
            {
                if (row.Count <= timeColumn)
                    throw new FormatException($"Spawn table row is missing the time column: {string.Join(" | ", row)}");
                if (!TimeOnly.TryParseExact(row[timeColumn], "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                    throw new FormatException($"Unrecognised spawn time '{row[timeColumn]}'.");

                var bosses = bossColumns
                    .Where(i => i < row.Count)
                    .Select(i => row[i])
                    .Where(b => b.Length > 0 && b != "-")
                    .ToList();
                if (bosses.Count > 0)
                    slots.Add(new BossSlot(time, bosses));
            }

            if (slots.Count < MinimumRows)
                throw new FormatException($"Spawn table has only {slots.Count} rows; expected at least {MinimumRows}.");
            return slots.OrderBy(s => s.UtcTime).ToList();
        }
        throw new FormatException("No spawn table with a 'Time (GMT)' column was found.");
    }

    private static List<string> Cells(string rowHtml) =>
        CellRegex().Matches(rowHtml)
            .Select(c => WebUtility.HtmlDecode(TagRegex().Replace(c.Groups[1].Value, "")).Trim())
            .ToList();

    public static string ToJson(IReadOnlyList<BossSlot> slots) =>
        JsonSerializer.Serialize(slots.Select(s => new SlotDto(s.UtcTime.ToString("HH:mm", CultureInfo.InvariantCulture), s.Bosses.ToList())).ToList());

    public static IReadOnlyList<BossSlot> FromJson(string json)
    {
        var dtos = JsonSerializer.Deserialize<List<SlotDto>>(json) ?? throw new FormatException("Empty schedule cache.");
        var slots = dtos.Select(d => new BossSlot(TimeOnly.ParseExact(d.UtcTime, "HH:mm", CultureInfo.InvariantCulture), d.Bosses)).ToList();
        if (slots.Count < MinimumRows)
            throw new FormatException("Schedule cache is incomplete.");
        return slots;
    }

    private sealed record SlotDto(string UtcTime, List<string> Bosses);

    [GeneratedRegex(@"<table\b.*?</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex TableRegex();

    [GeneratedRegex(@"<tr\b.*?</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex RowRegex();

    [GeneratedRegex(@"<t[dh]\b[^>]*>(.*?)</t[dh]>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex CellRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagRegex();
}
