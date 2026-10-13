using Microsoft.Win32;

namespace MelissiaBossTimer;

internal static class StartupRegistration
{
    public const string MinimizedArg = "--minimized";
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "MelissiaBossTimer";

    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(ValueName) is string;
        }
    }

    public static void Set(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
            key.SetValue(ValueName, $"\"{Environment.ProcessPath}\" {MinimizedArg}");
        else
            key.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
