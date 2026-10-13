namespace MelissiaBossTimer.Core;

public sealed record Alarm(Spawn Spawn, IReadOnlyList<string> Bosses);

/// <summary>Decides which spawns should alarm now. Each spawn alarms at most once.</summary>
public sealed class AlarmPlanner
{
    /// <summary>How late an alarm may still fire for a spawn that has already happened (e.g. lead time 0).</summary>
    public static readonly TimeSpan LateGrace = TimeSpan.FromSeconds(60);

    private readonly HashSet<(DateTime Utc, string Bosses)> _fired = [];

    /// <summary>
    /// Returns spawns whose alarm time (spawn - lead) has been reached and that have not spawned
    /// more than <see cref="LateGrace"/> ago. Spawns missed while the PC slept are therefore skipped
    /// rather than alarming hours late.
    /// </summary>
    public IReadOnlyList<Alarm> Due(IReadOnlyList<BossSlot> slots, DateTime utcNow, TimeSpan lead, Func<string, bool> isEnabled)
    {
        _fired.RemoveWhere(f => f.Utc < utcNow - TimeSpan.FromDays(1));

        var due = new List<Alarm>();
        foreach (var spawn in BossSchedule.Upcoming(slots, utcNow - LateGrace, LateGrace + lead + TimeSpan.FromSeconds(1)))
        {
            if (utcNow < spawn.Utc - lead)
                continue;

            var bosses = spawn.Slot.Bosses.Where(isEnabled).ToList();
            if (bosses.Count == 0)
                continue;

            if (_fired.Add((spawn.Utc, string.Join('|', spawn.Slot.Bosses))))
                due.Add(new Alarm(spawn, bosses));
        }
        return due;
    }
}
