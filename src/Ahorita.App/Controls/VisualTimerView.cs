namespace Ahorita.App.Controls;

/// <summary>
/// Anillo tipo "Time Timer" que se vacía: el tiempo restante se ve, no se lee.
/// El arco se dibuja en segmentos para simular un gradiente del color de la rutina.
/// </summary>
public sealed class VisualTimerView : GraphicsView, IDrawable
{
    private const int Segments = 72;

    public static readonly BindableProperty FractionProperty = Create(nameof(Fraction), typeof(double), 1d);
    public static readonly BindableProperty StartColorProperty = Create(nameof(StartColor), typeof(Color), Colors.Orange);
    public static readonly BindableProperty EndColorProperty = Create(nameof(EndColor), typeof(Color), Colors.OrangeRed);
    public static readonly BindableProperty TrackColorProperty = Create(nameof(TrackColor), typeof(Color), Colors.LightGray);
    public static readonly BindableProperty ThicknessProperty = Create(nameof(Thickness), typeof(double), 22d);

    public VisualTimerView()
    {
        Drawable = this;
        InputTransparent = true;
    }

    /// <summary>Tiempo restante de 0 a 1.</summary>
    public double Fraction
    {
        get => (double)GetValue(FractionProperty);
        set => SetValue(FractionProperty, value);
    }

    public Color StartColor
    {
        get => (Color)GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public Color EndColor
    {
        get => (Color)GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var thickness = (float)Thickness;
        var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
        var radius = (size - thickness) / 2f - 2f;
        var center = new PointF(dirtyRect.Center.X, dirtyRect.Center.Y);

        canvas.StrokeSize = thickness;
        canvas.StrokeColor = TrackColor;
        canvas.DrawCircle(center, radius);

        var fraction = Math.Clamp(Fraction, 0d, 1d);
        if (fraction <= 0)
            return;

        // De las 12 en punto, en sentido horario, hasta donde queda tiempo.
        var sweep = 360d * fraction;
        canvas.StrokeLineCap = LineCap.Butt;
        for (var i = 0; i < Segments; i++)
        {
            var from = sweep * i / Segments;
            var to = sweep * (i + 1) / Segments;
            canvas.StrokeColor = Lerp(StartColor, EndColor, (float)i / (Segments - 1));
            DrawArc(canvas, center, radius, from, to + 0.6);
        }

        // Extremos redondeados y "punta" brillante que marca el avance.
        canvas.FillColor = StartColor;
        canvas.FillCircle(PointAt(center, radius, 0), thickness / 2f);
        var tip = PointAt(center, radius, sweep);
        canvas.FillColor = EndColor;
        canvas.FillCircle(tip, thickness / 2f);
        canvas.FillColor = Colors.White.WithAlpha(0.9f);
        canvas.FillCircle(tip, thickness * 0.22f);
    }

    private static void DrawArc(ICanvas canvas, PointF center, float radius, double fromDegrees, double toDegrees)
    {
        // Maui.Graphics mide ángulos desde las 3 en punto, en sentido antihorario.
        var start = (float)(90 - fromDegrees);
        var end = (float)(90 - toDegrees);
        canvas.DrawArc(center.X - radius, center.Y - radius, radius * 2, radius * 2, start, end, clockwise: true, closed: false);
    }

    private static PointF PointAt(PointF center, float radius, double clockwiseDegreesFromTop)
    {
        var radians = (90 - clockwiseDegreesFromTop) * Math.PI / 180d;
        return new PointF(center.X + radius * (float)Math.Cos(radians), center.Y - radius * (float)Math.Sin(radians));
    }

    private static Color Lerp(Color from, Color to, float t) => new(
        from.Red + (to.Red - from.Red) * t,
        from.Green + (to.Green - from.Green) * t,
        from.Blue + (to.Blue - from.Blue) * t,
        from.Alpha + (to.Alpha - from.Alpha) * t);

    private static BindableProperty Create(string name, Type type, object defaultValue) =>
        BindableProperty.Create(name, type, typeof(VisualTimerView), defaultValue,
            propertyChanged: (bindable, _, _) => ((VisualTimerView)bindable).Invalidate());
}
