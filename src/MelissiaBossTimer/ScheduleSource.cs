using MelissiaBossTimer.Core;

namespace MelissiaBossTimer;

internal sealed record LoadedSchedule(IReadOnlyList<BossSlot> Slots, string Description);

/// <summary>Online schedule with a saved copy and the built-in snapshot as fallbacks.</summary>
internal static class ScheduleSource
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static string CachePath => Path.Combine(AppPaths.Folder, "schedule-cache.json");

    public static LoadedSchedule LoadOffline()
    {
        try
        {
            if (File.Exists(CachePath))
            {
                var slots = ScheduleParser.FromJson(File.ReadAllText(CachePath));
                return new LoadedSchedule(slots, $"saved copy from {File.GetLastWriteTime(CachePath):d MMM yyyy HH:mm}");
            }
        }
        catch (Exception ex) when (ex is IOException or FormatException or System.Text.Json.JsonException or UnauthorizedAccessException)
        {
            // Fall through to the built-in snapshot.
        }
        return new LoadedSchedule(BossSchedule.BuiltIn, "built-in copy (14 Oct 2026)");
    }

    /// <summary>Downloads and validates the wiki table. Throws on network or layout failure; the caller keeps its current schedule.</summary>
    public static async Task<LoadedSchedule> FetchOnlineAsync()
    {
        var html = await Http.GetStringAsync(BossSchedule.SourceUrl);
        var slots = ScheduleParser.ParseHtml(html);
        try
        {
            Directory.CreateDirectory(AppPaths.Folder);
            await File.WriteAllTextAsync(CachePath, ScheduleParser.ToJson(slots));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The fresh schedule is still usable for this session.
        }
        return new LoadedSchedule(slots, $"online copy, checked {DateTime.Now:HH:mm}");
    }
}
