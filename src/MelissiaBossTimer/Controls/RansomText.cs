using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MelissiaBossTimer.Controls;

/// <summary>
/// Ransom-note lettering: every glyph cut into its own tilted box (white, black or red) in mixed faces.
/// The cut is deterministic per text, so a name always looks the same; changing the text re-deals the letters with a snap-in.
/// Words separated by " · " get a red star between them.
/// </summary>
public sealed class RansomText : StackPanel
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(RansomText),
        new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsMeasure, (d, _) => ((RansomText)d).Deal()));

    public static readonly DependencyProperty LetterSizeProperty = DependencyProperty.Register(
        nameof(LetterSize), typeof(double), typeof(RansomText),
        new FrameworkPropertyMetadata(56.0, FrameworkPropertyMetadataOptions.AffectsMeasure, (d, _) => ((RansomText)d).Deal()));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double LetterSize
    {
        get => (double)GetValue(LetterSizeProperty);
        set => SetValue(LetterSizeProperty, value);
    }

    public RansomText()
    {
        Orientation = Orientation.Horizontal;
        VerticalAlignment = VerticalAlignment.Center;
    }

    private void Deal()
    {
        Children.Clear();
        var text = (Text ?? "").ToUpperInvariant();
        var rng = new Random(StableHash(text));
        var red = (Brush)Application.Current.FindResource("Red");
        var ink = (Brush)Application.Current.FindResource("Ink");
        var ground = (Brush)Application.Current.FindResource("Ground");
        var display = (FontFamily)Application.Current.FindResource("Display");
        var numerals = (FontFamily)Application.Current.FindResource("Numerals");

        var words = text.Split(" · ");
        var index = 0;
        for (var w = 0; w < words.Length; w++)
        {
            if (w > 0)
                Children.Add(Star(red));
            var startOfWord = true;
            foreach (var ch in words[w])
            {
                if (ch == ' ')
                {
                    Children.Add(new Border { Width = LetterSize * 0.22 });
                    startOfWord = true;
                    continue;
                }

                // First letter of each word is the loud one; the rest alternate so no two neighbours match.
                var roll = rng.NextDouble();
                var (fill, fore, stroke) = startOfWord
                    ? (red, ink, ground)
                    : roll < 0.45 ? (ink, ground, ground)
                    : roll < 0.8 ? (ground, ink, ink)
                    : (red, ink, ground);
                var size = LetterSize * (startOfWord ? 1.12 : 0.82 + rng.NextDouble() * 0.2);
                var face = rng.NextDouble() < 0.3 ? numerals : display;

                var glyph = new TextBlock
                {
                    Text = ch.ToString(),
                    FontFamily = face,
                    FontWeight = face == numerals ? FontWeights.ExtraBold : FontWeights.Normal,
                    FontSize = size,
                    Foreground = fore,
                    LineHeight = size * 1.02,
                    LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                };
                var box = new Border
                {
                    Background = fill,
                    BorderBrush = stroke,
                    BorderThickness = new Thickness(fill == ground ? 2 : 0),
                    Padding = new Thickness(size * 0.08, 0, size * 0.08, 0),
                    Margin = new Thickness(-1, 0, -1, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = glyph,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new TransformGroup
                    {
                        Children =
                        {
                            new ScaleTransform(1, 1),
                            new RotateTransform(-7 + rng.NextDouble() * 14),
                            new TranslateTransform(0, -LetterSize * 0.06 + rng.NextDouble() * LetterSize * 0.12),
                        },
                    },
                };
                SnapIn(box, index++);
                Children.Add(box);
                startOfWord = false;
            }
        }
    }

    private Shape Star(Brush fill)
    {
        var s = LetterSize * 0.42;
        var points = new PointCollection();
        for (var i = 0; i < 10; i++)
        {
            var r = i % 2 == 0 ? s / 2 : s / 5;
            var a = Math.PI / 5 * i - Math.PI / 2;
            points.Add(new Point(s / 2 + r * Math.Cos(a), s / 2 + r * Math.Sin(a)));
        }
        return new Polygon
        {
            Points = points,
            Fill = fill,
            Width = s,
            Height = s,
            Margin = new Thickness(LetterSize * 0.12, 0, LetterSize * 0.12, 0),
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new RotateTransform(12),
        };
    }

    private static void SnapIn(Border box, int index)
    {
        if (!SystemParameters.ClientAreaAnimation)
            return;
        var scale = (ScaleTransform)((TransformGroup)box.RenderTransform).Children[0];
        var begin = TimeSpan.FromMilliseconds(index * 16);
        var ease = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6 };
        var grow = new DoubleAnimation(1.6, 1, TimeSpan.FromMilliseconds(160)) { BeginTime = begin, EasingFunction = ease };
        box.Opacity = 0;
        box.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(60)) { BeginTime = begin });
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
    }

    private static int StableHash(string s)
    {
        unchecked
        {
            var h = 17;
            foreach (var c in s)
                h = h * 31 + c;
            return h;
        }
    }
}
