using System.Windows;
using MelissiaBossTimer.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MelissiaBossTimer.Core;
using Microsoft.Win32;

namespace MelissiaBossTimer.Pages;

/// <summary>The sound library: built-in tones and the user's own WAV/MP3 files, with preview, default and volume.</summary>
internal sealed class SoundsPage : UserControl, IPage
{
    private readonly AppState _state;
    private readonly StackPanel _library = new();
    private readonly ComboBox _default = new();
    private readonly Slider _volume = new() { Minimum = 0, Maximum = 100, SmallChange = 5, LargeChange = 10 };
    private readonly TextBlock _volumeText = new() { Style = (Style)Application.Current.FindResource("Time"), FontSize = 20, Width = 52, TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
    private readonly CheckBox _soundOn;
    private readonly TextBlock _message = new() { Style = (Style)Application.Current.FindResource("BodyText"), Margin = new Thickness(0, 10, 0, 0), Visibility = Visibility.Collapsed };
    private bool _updating;

    public SoundsPage(AppState state)
    {
        _state = state;
        Focusable = false;

        var layout = new Grid { Margin = new Thickness(22, 18, 0, 0) };
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
        layout.ColumnDefinitions.Add(new ColumnDefinition());

        var left = new StackPanel { Margin = new Thickness(0, 0, 26, 18) };
        left.Children.Add(new TextBlock { Text = "SOUNDS", Style = (Style)Application.Current.FindResource("PageTitle"), Margin = new Thickness(0, 0, 0, 14) });

        left.Children.Add(Ui.Label("DEFAULT ALARM SOUND"));
        _default.SelectionChanged += (_, _) =>
        {
            if (_updating || _default.SelectedItem is not SoundOption option)
                return;
            _state.Settings.DefaultSoundId = option.Id;
            _state.Save();
        };
        System.Windows.Automation.AutomationProperties.SetName(_default, "Default alarm sound");
        left.Children.Add(_default);

        var volumeLabel = Ui.Label("VOLUME");
        volumeLabel.Margin = new Thickness(0, 16, 0, 6);
        left.Children.Add(volumeLabel);
        var volumeRow = new DockPanel();
        DockPanel.SetDock(_volumeText, Dock.Right);
        volumeRow.Children.Add(_volumeText);
        volumeRow.Children.Add(_volume);
        System.Windows.Automation.AutomationProperties.SetName(_volume, "Alarm volume");
        _volume.ValueChanged += (_, e) =>
        {
            _volumeText.Text = $"{(int)e.NewValue}%";
            if (_updating)
                return;
            _state.Settings.Volume = (int)e.NewValue;
            _state.Player.SetVolume(_state.Settings.Volume);
        };
        _volume.LostMouseCapture += (_, _) => _state.Save();
        _volume.KeyUp += (_, _) => _state.Save();
        left.Children.Add(volumeRow);
        var volumeNote = Ui.Body("Separate from Windows volume. Previews play at this level.");
        volumeNote.FontSize = 13;
        volumeNote.Margin = new Thickness(0, 4, 0, 0);
        left.Children.Add(volumeNote);

        _soundOn = Ui.Switch("Play a sound when an alarm fires", true, on =>
        {
            _state.Settings.Sound = on;
            _state.Save();
        });
        _soundOn.Margin = new Thickness(0, 16, 0, 0);
        left.Children.Add(_soundOn);

        var add = Ui.Button("Add sound", (_, _) => AddSound(), glyph: Ui.Glyph.Add);
        add.HorizontalAlignment = HorizontalAlignment.Left;
        add.Margin = new Thickness(0, 24, 0, 0);
        left.Children.Add(add);
        var addNote = Ui.Body("A short .wav or .mp3 (up to 20 MB). It's copied into the app, so moving the original won't break the alarm.");
        addNote.FontSize = 13;
        addNote.Margin = new Thickness(0, 8, 0, 0);
        left.Children.Add(addNote);
        left.Children.Add(_message);

        layout.Children.Add(new ScrollViewer { Content = left, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });

        var right = new ScrollViewer { Content = _library, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(0, 4, 22, 18) };
        Grid.SetColumn(right, 1);
        layout.Children.Add(right);
        Content = layout;
    }

    private void AddSound()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose an alarm sound",
            Filter = "Sound files (*.wav;*.mp3)|*.wav;*.mp3",
            CheckFileExists = true,
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
            return;
        try
        {
            var sound = _state.Sounds.Import(dialog.FileName, _state.Settings);
            ShowMessage($"Added “{sound.Name}”. Preview it below, or pick it for a boss in Bosses.", error: false);
            _state.Save();
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            ShowMessage(ex.Message, error: true);
        }
    }

    private void ShowMessage(string text, bool error)
    {
        _message.Text = text;
        _message.Foreground = Ui.Brush(error ? "Red" : "Ink");
        _message.Visibility = Visibility.Visible;
    }

    public void Refresh()
    {
        _updating = true;
        var settings = _state.Settings;
        var options = _state.SoundOptions;
        _default.ItemsSource = options;
        _default.SelectedItem = options.FirstOrDefault(o => o.Id == settings.DefaultSoundId) ?? options[0];
        _volume.Value = settings.Volume;
        _volumeText.Text = $"{settings.Volume}%";
        _soundOn.IsChecked = settings.Sound;
        _updating = false;

        if (_state.LastError is { } error && error.StartsWith("Couldn't play", StringComparison.Ordinal))
            ShowMessage(error, error: true);

        _library.Children.Clear();
        _library.Children.Add(Ui.Tab("Built in"));
        var builtIn = new StackPanel { Margin = new Thickness(0, 4, 0, 20) };
        foreach (var sound in BuiltInSounds.All)
            builtIn.Children.Add(Row(sound.Id, sound.Name, sound.Description, custom: false));
        _library.Children.Add(builtIn);

        _library.Children.Add(Ui.Tab("Your sounds"));
        var mine = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };
        if (settings.CustomSounds.Count == 0)
        {
            var empty = new Grid { Margin = new Thickness(0, 4, 0, 0) };
            empty.Children.Add(new Rectangle { Stroke = Ui.Brush("Dim"), StrokeThickness = 1, StrokeDashArray = [4, 3] });
            var text = Ui.Body("No sounds of your own yet. Use Add sound to bring in a clip: a guild horn, a voice line, anything short and loud.");
            text.Margin = new Thickness(16, 14, 16, 14);
            empty.Children.Add(text);
            mine.Children.Add(empty);
        }
        foreach (var sound in settings.CustomSounds)
            mine.Children.Add(Row(sound.Id, sound.Name, "Your file", custom: true));
        _library.Children.Add(mine);
    }

    private FrameworkElement Row(string id, string name, string description, bool custom)
    {
        var settings = _state.Settings;
        var playing = _state.IsPreviewing(id);
        var isDefault = settings.DefaultSoundId == id;
        var usedBy = settings.BossSounds.Where(p => p.Value == id).Select(p => p.Key).Order().ToList();

        var grid = new Grid { MinHeight = 58 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var preview = Ui.IconButton(playing ? Ui.Glyph.Stop : Ui.Glyph.Play, playing ? $"Stop {name}" : $"Preview {name}", (_, _) => _state.PreviewOrStop(id));
        preview.Margin = new Thickness(10, 0, 12, 0);
        preview.VerticalAlignment = VerticalAlignment.Center;
        if (playing)
            preview.Background = Ui.Brush("Red");
        grid.Children.Add(preview);

        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 6, 12, 6) };
        text.Children.Add(new TextBlock { Text = name, FontFamily = Ui.Font("Body"), FontWeight = FontWeights.Bold, FontSize = 16, Foreground = Ui.Brush("Ink"), TextTrimming = TextTrimming.CharacterEllipsis });
        var detail = usedBy.Count > 0 ? $"{description} · rings for {string.Join(", ", usedBy)}" : description;
        var sub = Ui.Body(detail);
        sub.FontSize = 13;
        sub.TextWrapping = TextWrapping.NoWrap;
        sub.TextTrimming = TextTrimming.CharacterEllipsis;
        text.Children.Add(sub);
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
        if (isDefault)
        {
            var badge = new Grid { VerticalAlignment = VerticalAlignment.Center };
            badge.Children.Add(new Shard { Points = [new(0.1, 0), new(1, 0), new(0.9, 1), new(0, 1)], Fill = Ui.Brush("Ink") });
            badge.Children.Add(new TextBlock { Text = "DEFAULT", FontFamily = Ui.Font("Display"), FontSize = 14, Foreground = Ui.Brush("Ground"), Margin = new Thickness(12, 2, 12, 3) });
            actions.Children.Add(badge);
        }
        else
        {
            var use = new Button { Style = (Style)Application.Current.FindResource("LinkButton"), Content = "Use as default", VerticalAlignment = VerticalAlignment.Center };
            use.Click += (_, _) =>
            {
                settings.DefaultSoundId = id;
                _state.Save();
            };
            actions.Children.Add(use);
        }
        if (custom)
        {
            var delete = Ui.IconButton(Ui.Glyph.Delete, $"Remove {name}", (_, _) =>
            {
                if (_state.IsPreviewing(id))
                    _state.Player.Stop();
                _state.Sounds.Remove(id, settings);
                ShowMessage($"Removed “{name}”. Bosses that used it are back on the default sound.", error: false);
                _state.Save();
            });
            delete.Margin = new Thickness(14, 0, 0, 0);
            actions.Children.Add(delete);
        }
        Grid.SetColumn(actions, 2);
        grid.Children.Add(actions);

        return new Border
        {
            Background = Ui.Brush(playing ? "Surface2" : "Surface"),
            BorderBrush = Ui.Brush("Ground"),
            BorderThickness = new Thickness(0, 0, 0, 2),
            Child = grid,
        };
    }
}
