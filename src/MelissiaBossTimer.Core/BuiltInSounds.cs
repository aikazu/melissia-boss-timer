using System.Text;

namespace MelissiaBossTimer.Core;

public sealed record BuiltInSound(string Id, string Name, string Description);

/// <summary>Alarm tones synthesized in code, so the app ships no audio assets. Each renders as a loopable 16-bit mono WAV.</summary>
public static class BuiltInSounds
{
    public const string DefaultId = "builtin:beacon";
    private const int SampleRate = 44100;

    public static IReadOnlyList<BuiltInSound> All { get; } =
    [
        new("builtin:beacon", "Beacon", "Two short beeps and a high one"),
        new("builtin:war-horn", "War horn", "Low brass blast, cuts through game audio"),
        new("builtin:temple-bell", "Temple bell", "Struck bell with a long ring"),
        new("builtin:chime", "Chime", "Soft rising three-note chime"),
        new("builtin:siren", "Siren", "Rising and falling sweep"),
    ];

    public static bool IsBuiltIn(string id) => All.Any(s => s.Id == id);

    public static byte[] RenderWav(string id)
    {
        var samples = id switch
        {
            "builtin:beacon" => Beacon(),
            "builtin:war-horn" => WarHorn(),
            "builtin:temple-bell" => TempleBell(),
            "builtin:chime" => Chime(),
            "builtin:siren" => Siren(),
            _ => throw new ArgumentException($"Unknown built-in sound '{id}'.", nameof(id)),
        };
        return ToWav(Normalize(samples, 0.85));
    }

    private static List<double> Beacon()
    {
        var s = new List<double>();
        foreach (var (hz, ms) in new (double, int)[] { (880, 150), (0, 80), (880, 150), (0, 80), (1320, 300), (0, 600) })
            s.AddRange(hz == 0 ? Silence(ms) : Tone(ms, t => Math.Sin(2 * Math.PI * hz * t), fadeMs: 10));
        return s;
    }

    private static List<double> WarHorn()
    {
        // Sawtooth-like brass from decaying harmonics, slow attack and a little vibrato; short blast then a long one.
        static IEnumerable<double> Blast(int ms) => Tone(ms, t =>
        {
            var f = 146.83 * (1 + 0.006 * Math.Sin(2 * Math.PI * 5.5 * t));
            var v = 0.0;
            for (var h = 1; h <= 8; h++)
                v += Math.Sin(2 * Math.PI * f * h * t) / h;
            return v;
        }, fadeMs: 70);
        var s = new List<double>();
        s.AddRange(Blast(380));
        s.AddRange(Silence(120));
        s.AddRange(Blast(1100));
        s.AddRange(Silence(700));
        return s;
    }

    private static List<double> TempleBell()
    {
        // Inharmonic partials of a struck bell, each decaying at its own rate.
        (double Ratio, double Amp, double Decay)[] partials = [(0.5, 0.5, 1.2), (1, 1, 1.6), (2.76, 0.5, 3), (5.4, 0.25, 5), (8.93, 0.12, 8)];
        const double f = 392;
        var s = Tone(2600, t => partials.Sum(p => p.Amp * Math.Exp(-p.Decay * t) * Math.Sin(2 * Math.PI * f * p.Ratio * t)), fadeMs: 2).ToList();
        s.AddRange(Silence(300));
        return s;
    }

    private static List<double> Chime()
    {
        var s = new List<double>();
        foreach (var hz in new[] { 659.25, 830.61, 987.77 })
            s.AddRange(Tone(420, t => Math.Exp(-4 * t) * (Math.Sin(2 * Math.PI * hz * t) + 0.3 * Math.Sin(2 * Math.PI * hz * 2 * t)), fadeMs: 3));
        s.AddRange(Tone(900, t => Math.Exp(-3 * t) * Math.Sin(2 * Math.PI * 1318.5 * t), fadeMs: 3));
        s.AddRange(Silence(700));
        return s;
    }

    private static List<double> Siren()
    {
        // Phase-accumulated sweep 600 -> 1200 -> 600 Hz so the pitch glides without clicks.
        var s = new List<double>();
        var phase = 0.0;
        var n = SampleRate * 1400 / 1000;
        for (var i = 0; i < n; i++)
        {
            var x = (double)i / n;
            var hz = 600 + 600 * Math.Sin(Math.PI * x);
            phase += 2 * Math.PI * hz / SampleRate;
            var env = Math.Min(1, Math.Min(i, n - i) / (SampleRate * 0.02));
            s.Add(env * (Math.Sin(phase) + 0.2 * Math.Sin(3 * phase)));
        }
        s.AddRange(Silence(400));
        return s;
    }

    private static IEnumerable<double> Tone(int ms, Func<double, double> wave, int fadeMs)
    {
        var n = SampleRate * ms / 1000;
        var fade = Math.Max(1, SampleRate * fadeMs / 1000.0);
        for (var i = 0; i < n; i++)
            yield return wave((double)i / SampleRate) * Math.Min(1, Math.Min(i, n - i) / fade);
    }

    private static IEnumerable<double> Silence(int ms) => Enumerable.Repeat(0.0, SampleRate * ms / 1000);

    private static List<double> Normalize(List<double> s, double peak)
    {
        var max = s.Max(Math.Abs);
        return max == 0 ? s : s.Select(v => v / max * peak).ToList();
    }

    private static byte[] ToWav(List<double> samples)
    {
        using var stream = new MemoryStream();
        using (var w = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true))
        {
            var dataBytes = samples.Count * 2;
            w.Write("RIFF"u8);
            w.Write(36 + dataBytes);
            w.Write("WAVEfmt "u8);
            w.Write(16);
            w.Write((short)1);          // PCM
            w.Write((short)1);          // mono
            w.Write(SampleRate);
            w.Write(SampleRate * 2);    // byte rate
            w.Write((short)2);          // block align
            w.Write((short)16);         // bits per sample
            w.Write("data"u8);
            w.Write(dataBytes);
            foreach (var v in samples)
                w.Write((short)Math.Round(v * short.MaxValue));
        }
        return stream.ToArray();
    }
}
