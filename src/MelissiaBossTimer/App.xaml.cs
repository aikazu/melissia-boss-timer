using System.Windows;

namespace MelissiaBossTimer;

public partial class App : Application
{
    private const string InstanceName = "MelissiaBossTimer.SingleInstance";

    private Mutex? _mutex;
    private EventWaitHandle? _showSignal;
    private RegisteredWaitHandle? _listener;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            // A UI bug must not take the alarms down with it: log it and keep running.
            try
            {
                Directory.CreateDirectory(AppPaths.Folder);
                File.AppendAllText(Path.Combine(AppPaths.Folder, "error.log"), $"{DateTime.Now:O} {args.Exception}{Environment.NewLine}");
            }
            catch (IOException)
            {
            }
            args.Handled = true;
        };

        // A second copy would double every alarm, so hand off to the running one instead.
        _mutex = new Mutex(true, InstanceName + ".Mutex", out var isFirst);
        _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, InstanceName + ".Show");
        if (!isFirst)
        {
            _showSignal.Set();
            Shutdown();
            return;
        }

        var startHidden = e.Args.Contains(StartupRegistration.MinimizedArg, StringComparer.OrdinalIgnoreCase);
        var window = new MainWindow();
        _listener = ThreadPool.RegisterWaitForSingleObject(_showSignal,
            (_, _) => Dispatcher.BeginInvoke(window.ShowFromTray), null, Timeout.Infinite, executeOnlyOnce: false);

        if (!startHidden)
            window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _listener?.Unregister(null);
        _showSignal?.Dispose();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
