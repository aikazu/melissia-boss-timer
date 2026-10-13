namespace MelissiaBossTimer;

internal static class AppPaths
{
    public static string Folder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MelissiaBossTimer");

    public static string Settings => Path.Combine(Folder, "settings.json");
}
