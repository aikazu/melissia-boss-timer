using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MelissiaBossTimer;

/// <summary>Paints the native title bar to match the app's black ground (Windows 11; ignored elsewhere).</summary>
internal static class DarkTitleBar
{
    private const int DwmUseImmersiveDarkMode = 20;
    private const int DwmCaptionColor = 35;
    private const int DwmTextColor = 36;

    public static void Apply(Window window)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        var on = 1;
        DwmSetWindowAttribute(hwnd, DwmUseImmersiveDarkMode, ref on, sizeof(int));
        var caption = 0x000D0B0B; // COLORREF is 0x00BBGGRR: #0B0B0D
        DwmSetWindowAttribute(hwnd, DwmCaptionColor, ref caption, sizeof(int));
        var text = 0x00FFFFFF;
        DwmSetWindowAttribute(hwnd, DwmTextColor, ref text, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
