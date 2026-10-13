using MelissiaBossTimer.Core;

namespace MelissiaBossTimer.Tests;

public class ScheduleTests
{
    private static readonly TimeZoneInfo Jakarta = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    private static readonly TimeZoneInfo Pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

    private static DateTime Utc(int y, int mo, int d, int h, int mi) => new(y, mo, d, h, mi, 0, DateTimeKind.Utc);

    [Fact]
    public void Late_utc_spawn_lands_on_next_local_day_in_gmt_plus_7()
    {
        var spawns = BossSchedule.Upcoming(BossSchedule.BuiltIn, Utc(2026, 10, 14, 21, 0), TimeSpan.FromHours(2));

        var kzarka = spawns.First(s => s.Slot.Bosses.Contains("Kzarka"));
        Assert.Equal(new DateTime(2026, 10, 15, 5, 0, 0), kzarka.ToLocal(Jakarta));
    }

    [Fact]
    public void Same_slot_shifts_local_time_across_a_dst_transition()
    {
        // US DST ends 2026-11-01: 22:00 UTC is 15:00 PDT the day before, 14:00 PST the day after.
        var before = BossSchedule.Upcoming(BossSchedule.BuiltIn, Utc(2026, 10, 31, 0, 0), TimeSpan.FromDays(1));
        var after = BossSchedule.Upcoming(BossSchedule.BuiltIn, Utc(2026, 11, 2, 0, 0), TimeSpan.FromDays(1));

        Assert.Equal(15, before.Single(s => s.Utc.Hour == 22 && s.Utc.Minute == 0).ToLocal(Pacific).Hour);
        Assert.Equal(14, after.Single(s => s.Utc.Hour == 22 && s.Utc.Minute == 0).ToLocal(Pacific).Hour);
    }

    [Fact]
    public void Upcoming_wraps_past_utc_midnight_in_order()
    {
        var spawns = BossSchedule.Upcoming(BossSchedule.BuiltIn, Utc(2026, 10, 14, 22, 50), TimeSpan.FromHours(4));

        Assert.Equal(Utc(2026, 10, 15, 2, 0), spawns[0].Utc);
        Assert.Equal(["Bulgasal"], spawns[0].Slot.Bosses);
        Assert.Equal(spawns.OrderBy(s => s.Utc).Select(s => s.Utc), spawns.Select(s => s.Utc));
    }

    [Fact]
    public void Full_day_contains_every_slot_once()
    {
        var spawns = BossSchedule.Upcoming(BossSchedule.BuiltIn, Utc(2026, 10, 14, 13, 7), TimeSpan.FromDays(1));

        Assert.Equal(BossSchedule.BuiltIn.Count, spawns.Count);
    }

    [Fact]
    public void Regulars_are_the_four_hour_field_bosses()
    {
        var regulars = BossSchedule.AllBosses(BossSchedule.BuiltIn).Where(b => BossSchedule.IsRegular(BossSchedule.BuiltIn, b));

        Assert.Equal(["Bulgasal", "Golden Pig King", "Sangoon", "Uturi"], regulars);
    }

    [Fact]
    public void Day_parts_follow_local_time_and_split_across_midnight()
    {
        // 04:52 local in GMT+7.
        var spawns = BossSchedule.Upcoming(BossSchedule.BuiltIn, Utc(2026, 10, 13, 21, 52), TimeSpan.FromDays(1));

        var parts = BossSchedule.GroupByDayPart(spawns, Jakarta);

        Assert.Equal(["Dawn", "Morning", "Afternoon", "Evening", "Night"], parts.Select(p => p.Name));
        Assert.Equal(4, parts[0].Spawns.Count);
        Assert.Equal(new DateTime(2026, 10, 15), parts[^1].LocalDate);
        Assert.Equal(spawns.Count, parts.Sum(p => p.Spawns.Count));
    }

    [Fact]
    public void Parses_the_live_page_snapshot_and_matches_built_in()
    {
        var html = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "boss-schedule.html"));

        var parsed = ScheduleParser.ParseHtml(html);

        Assert.Equal(32, parsed.Count);
        Assert.Equal(
            BossSchedule.BuiltIn.Select(s => $"{s.UtcTime:HH:mm} {string.Join(",", s.Bosses)}"),
            parsed.Select(s => $"{s.UtcTime:HH:mm} {string.Join(",", s.Bosses)}"));
    }

    [Fact]
    public void Rejects_html_without_a_gmt_table()
    {
        Assert.Throws<FormatException>(() => ScheduleParser.ParseHtml("<table><tr><th>Foo</th></tr></table>"));
    }

    [Fact]
    public void Json_cache_round_trips()
    {
        var restored = ScheduleParser.FromJson(ScheduleParser.ToJson(BossSchedule.BuiltIn));

        Assert.Equal(BossSchedule.BuiltIn.Select(s => (s.UtcTime, string.Join(",", s.Bosses))),
                     restored.Select(s => (s.UtcTime, string.Join(",", s.Bosses))));
    }
}

public class AlarmPlannerTests
{
    private static readonly TimeSpan Lead = TimeSpan.FromMinutes(5);
    private static DateTime Utc(int h, int mi, int s = 0) => new(2026, 10, 14, h, mi, s, DateTimeKind.Utc);

    [Fact]
    public void Fires_once_when_lead_time_is_reached()
    {
        var planner = new AlarmPlanner();

        Assert.Empty(planner.Due(BossSchedule.BuiltIn, Utc(10, 54, 59), Lead, _ => true));
        var due = planner.Due(BossSchedule.BuiltIn, Utc(10, 55), Lead, _ => true);
        Assert.Equal(["Kzarka"], Assert.Single(due).Bosses);
        Assert.Empty(planner.Due(BossSchedule.BuiltIn, Utc(10, 56), Lead, _ => true));
    }

    [Fact]
    public void Skips_spawns_missed_while_asleep()
    {
        var planner = new AlarmPlanner();

        // Resuming at 11:30 must not alarm for Kzarka at 11:00.
        Assert.Empty(planner.Due(BossSchedule.BuiltIn, Utc(11, 30), Lead, _ => true));
    }

    [Fact]
    public void Only_lists_enabled_bosses_and_skips_fully_disabled_slots()
    {
        var planner = new AlarmPlanner();

        Assert.Empty(planner.Due(BossSchedule.BuiltIn, Utc(10, 10), Lead, b => b != "Uturi"));
        var due = planner.Due(BossSchedule.BuiltIn, Utc(9, 56), Lead, b => b == "Vell");
        Assert.Equal(["Vell"], Assert.Single(due).Bosses);
    }

    [Fact]
    public void Zero_lead_fires_at_spawn_within_grace()
    {
        var planner = new AlarmPlanner();

        Assert.Single(planner.Due(BossSchedule.BuiltIn, Utc(11, 0, 30), TimeSpan.Zero, _ => true));
    }
}
