using System.Windows;
using System.Windows.Media;

namespace MelissiaBossTimer.Controls;

/// <summary>
/// A polygon in 0..1 coordinates that fills whatever box it is given but never asks for space itself,
/// so the content beside it sets the size. (A stretched Polygon wants all the height it's offered.)
/// It draws itself rather than deriving from Shape: Shape caches its geometry during measure/arrange,
/// and with a zero-size measure that cache went stale inside templates, so button plates never painted.
/// </summary>
public sealed class Shard : FrameworkElement
{
    private const FrameworkPropertyMetadataOptions Render = FrameworkPropertyMetadataOptions.AffectsRender;

    public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(
        nameof(Points), typeof(PointCollection), typeof(Shard), new FrameworkPropertyMetadata(null, Render));

    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill), typeof(Brush), typeof(Shard), new FrameworkPropertyMetadata(null, Render));

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
        nameof(Stroke), typeof(Brush), typeof(Shard), new FrameworkPropertyMetadata(null, Render));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness), typeof(double), typeof(Shard), new FrameworkPropertyMetadata(1.0, Render));

    public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
        nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Shard), new FrameworkPropertyMetadata(null, Render));

    public PointCollection? Points
    {
        get => (PointCollection?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public Brush? Stroke
    {
        get => (Brush?)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public DoubleCollection? StrokeDashArray
    {
        get => (DoubleCollection?)GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize) => new(0, 0);

    protected override Size ArrangeOverride(Size finalSize) => finalSize;

    protected override void OnRender(DrawingContext dc)
    {
        var points = Points;
        var size = RenderSize;
        if (points is null || points.Count < 3 || size.Width <= 0 || size.Height <= 0)
            return;

        var pen = Stroke is null || StrokeThickness <= 0 ? null : new Pen(Stroke, StrokeThickness)
        {
            LineJoin = PenLineJoin.Miter,
            DashStyle = StrokeDashArray is { Count: > 0 } dashes ? new DashStyle(dashes, 0) : DashStyles.Solid,
        };
        // Inset by half the stroke so the outline stays inside the box, like a stretched Shape.
        var inset = pen is null ? 0 : StrokeThickness / 2;
        var w = Math.Max(0, size.Width - 2 * inset);
        var h = Math.Max(0, size.Height - 2 * inset);

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(inset + points[0].X * w, inset + points[0].Y * h), isFilled: true, isClosed: true);
            for (var i = 1; i < points.Count; i++)
                ctx.LineTo(new Point(inset + points[i].X * w, inset + points[i].Y * h), isStroked: true, isSmoothJoin: false);
        }
        geometry.Freeze();
        dc.DrawGeometry(Fill, pen, geometry);
    }
}
