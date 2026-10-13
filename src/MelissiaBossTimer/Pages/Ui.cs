using System.Windows;
using MelissiaBossTimer.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MelissiaBossTimer.Pages;

/// <summary>Small builders for the repeated pieces pages draw from data.</summary>
internal static class Ui
{
    public static Brush Brush(string key) => (Brush)Application.Current.FindResource(key);

    public static FontFamily Font(string key) => (FontFamily)Application.Current.FindResource(key);

    /// <summary>Boss chip: major = red, regular = white, alarm off = dashed outline. On an inverted (red) row majors flip to white-on-black.</summary>
    public static FrameworkElement Chip(string boss, bool major, bool enabled, bool onRed = false)
    {
        var text = new TextBlock
        {
            Text = boss,
            Style = (Style)Application.Current.FindResource("ChipText"),
            Margin = new Thickness(8, 2, 8, 3),
            TextWrapping = TextWrapping.Wrap,
        };
        if (!enabled)
        {
            text.Foreground = Brush(onRed ? "Ink" : "Mute");
            return new Grid
            {
                Margin = new Thickness(0, 2, 6, 2),
                ToolTip = $"{boss}: alarm off",
                Children =
                {
                    new Rectangle { Stroke = Brush(onRed ? "Ink" : "Dim"), StrokeThickness = 1, StrokeDashArray = [3, 2] },
                    text,
                },
            };
        }

        var (fill, fore) = (major, onRed) switch
        {
            (true, false) => ("Red", "Ink"),
            (true, true) => ("Ground", "Ink"),
            (false, _) => ("Ink", "Ground"),
        };
        text.Foreground = Brush(fore);
        return new Border { Background = Brush(fill), Margin = new Thickness(0, 2, 6, 2), Child = text, ToolTip = boss };
    }

    /// <summary>A skewed red tab with a title, the header shape for every block.</summary>
    public static FrameworkElement Tab(string title, string? aside = null, double size = 20)
    {
        var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
        grid.Children.Add(new Shard
        {
            Points = [new(0.04, 0), new(1, 0), new(0.96, 1), new(0, 1)],
            Fill = Brush("Red"),
        });
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12, 2, 18, 3) };
        row.Children.Add(new TextBlock { Text = title.ToUpperInvariant(), FontFamily = Font("Display"), FontSize = size, Foreground = Brush("Ink") });
        if (aside is not null)
        {
            row.Children.Add(new TextBlock
            {
                Text = aside,
                FontFamily = Font("Numerals"),
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                Foreground = Brush("Ink"),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Typography = { NumeralAlignment = FontNumeralAlignment.Tabular },
            });
        }
        grid.Children.Add(row);
        return grid;
    }

    /// <summary>
    /// A boss face cut into a red shard. The art is white-and-black, so the red plate is what makes it read
    /// on the dark ground. Bosses without art get their initial instead.
    /// </summary>
    public static FrameworkElement Portrait(string boss, double size, bool dimmed = false, string plate = "Red")
    {
        Point[] shape = [new(0.1, 0), new(1, 0.04), new(0.9, 1), new(0, 0.95)];
        var grid = new Grid { Width = size, Height = size, ToolTip = boss };
        grid.Children.Add(new Shard { Points = [.. shape], Fill = Brush(dimmed ? "Off" : plate) });

        if (BossImages.For(boss, size) is { } source)
        {
            var clip = new PathGeometry([new PathFigure(Scale(shape[0]), shape.Skip(1).Select(p => (PathSegment)new LineSegment(Scale(p), false)), closed: true)]);
            clip.Freeze();
            grid.Children.Add(new Image { Source = source, Stretch = Stretch.UniformToFill, Clip = clip, Opacity = dimmed ? 0.55 : 1 });
        }
        else
        {
            grid.Children.Add(new TextBlock
            {
                Text = boss.Trim().FirstOrDefault(char.IsLetter).ToString().ToUpperInvariant(),
                FontFamily = Font("Display"),
                FontSize = size * 0.55,
                Foreground = Brush("Ink"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        System.Windows.Automation.AutomationProperties.SetName(grid, $"{boss} portrait");
        return grid;

        Point Scale(Point p) => new(p.X * size, p.Y * size);
    }

    /// <summary>
    /// A boss face laid straight onto the red banner, no plate. Art that runs off its canvas is cut by a hard slash
    /// parallel to the banner's slant on the left; the banner's own edges crop the top and bottom.
    /// </summary>
    public static FrameworkElement? Cutout(string boss, double size)
    {
        if (BossImages.For(boss, size) is not { } source)
            return null;
        var slash = new PathGeometry([new PathFigure(new Point(size * 0.22, 0),
        [
            new LineSegment(new Point(size, 0), false),
            new LineSegment(new Point(size, size), false),
            new LineSegment(new Point(size * 0.04, size), false),
        ], closed: true)]);
        slash.Freeze();
        var image = new Image { Source = source, Width = size, Height = size, Clip = slash, ToolTip = boss };
        System.Windows.Automation.AutomationProperties.SetName(image, $"{boss} portrait");
        return image;
    }

    public static TextBlock Label(string text) => new() { Text = text, Style = (Style)Application.Current.FindResource("FieldLabel") };

    public static TextBlock Body(string text) => new() { Text = text, Style = (Style)Application.Current.FindResource("BodyText") };

    /// <summary>Segoe Fluent icon glyph for buttons (not Unicode symbols standing in for icons).</summary>
    public static class Glyph
    {
        public const string Play = "";
        public const string Stop = "";
        public const string Delete = "";
        public const string Add = "";
        public const string Refresh = "";
        public const string Folder = "";
        public const string Link = "";
        public const string Bell = "";
    }

    public static Button IconButton(string glyph, string tooltip, RoutedEventHandler click)
    {
        var button = new Button
        {
            Style = (Style)Application.Current.FindResource("IconButton"),
            Content = glyph,
            ToolTip = tooltip,
        };
        System.Windows.Automation.AutomationProperties.SetName(button, tooltip);
        button.Click += click;
        return button;
    }

    /// <summary>A text button; a leading icon (a <see cref="Glyph"/>) helps it read as an action rather than a heading.</summary>
    public static Button Button(string text, RoutedEventHandler click, bool primary = true, string? glyph = null)
    {
        object content = text.ToUpperInvariant();
        if (glyph is not null)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(new TextBlock { Text = glyph, FontFamily = Font("Icons"), FontWeight = FontWeights.Normal, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 1, 8, 0) });
            row.Children.Add(new TextBlock { Text = text.ToUpperInvariant(), VerticalAlignment = VerticalAlignment.Center });
            content = row;
        }
        var button = new Button
        {
            Style = (Style)Application.Current.FindResource(primary ? "PrimaryButton" : "GhostButton"),
            Content = content,
        };
        System.Windows.Automation.AutomationProperties.SetName(button, text);
        button.Click += click;
        return button;
    }

    public static CheckBox Switch(string label, bool isOn, Action<bool> changed)
    {
        var box = new CheckBox
        {
            Style = (Style)Application.Current.FindResource("Switch"),
            Content = label,
            IsChecked = isOn,
        };
        box.Click += (_, _) => changed(box.IsChecked == true);
        return box;
    }
}
