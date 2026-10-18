using Ahorita.App.Theming;
using Ahorita.Domain.Routines;

namespace Ahorita.App.Controls;

/// <summary>
/// Confeti propio (sin dependencias): una ráfaga corta de ~2,5 s con los colores de
/// las rutinas. No hace nada si el movimiento está reducido.
/// </summary>
public sealed class ConfettiView : GraphicsView, IDrawable
{
    private const int ParticleCount = 110;
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(2.6);

    private readonly List<Particle> _particles = [];
    private IDispatcherTimer? _timer;
    private DateTime _startedAt;

    public ConfettiView()
    {
        Drawable = this;
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        Unloaded += (_, _) => Stop();
    }

    public void Burst()
    {
        if (Motion.IsReduced || Width <= 0 || Dispatcher is null)
            return;

        var colors = Enum.GetValues<RoutineAccent>().SelectMany(a => new[] { AccentPalette.For(a).Start, AccentPalette.For(a).End }).ToArray();
        var random = Random.Shared;

        _particles.Clear();
        for (var i = 0; i < ParticleCount; i++)
        {
            _particles.Add(new Particle
            {
                X = (float)(random.NextDouble() * Width),
                Y = (float)(-random.NextDouble() * Height * 0.3),
                VelocityX = (float)(random.NextDouble() - 0.5) * 220f,
                VelocityY = (float)(random.NextDouble() * 180 + 120),
                Rotation = (float)(random.NextDouble() * 360),
                Spin = (float)(random.NextDouble() - 0.5) * 540f,
                Size = (float)(random.NextDouble() * 6 + 6),
                Color = colors[random.Next(colors.Length)],
            });
        }

        Stop();
        _startedAt = DateTime.UtcNow;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        var last = _startedAt;
        _timer.Tick += (_, _) =>
        {
            var now = DateTime.UtcNow;
            Step((float)(now - last).TotalSeconds);
            last = now;
            Invalidate();
            if (now - _startedAt > Duration)
                Stop();
        };
        _timer.Start();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_timer is null)
            return;

        var fade = 1f - (float)Math.Clamp((DateTime.UtcNow - _startedAt - Duration * 0.7) / (Duration * 0.3), 0d, 1d);
        foreach (var p in _particles)
        {
            canvas.SaveState();
            canvas.Translate(p.X, p.Y);
            canvas.Rotate(p.Rotation);
            canvas.FillColor = p.Color.WithAlpha(fade);
            canvas.FillRoundedRectangle(-p.Size / 2, -p.Size / 4, p.Size, p.Size / 2, 2);
            canvas.RestoreState();
        }
    }

    private void Step(float seconds)
    {
        foreach (var p in _particles)
        {
            p.VelocityY += 420f * seconds;
            p.VelocityX *= 0.99f;
            p.X += p.VelocityX * seconds;
            p.Y += p.VelocityY * seconds;
            p.Rotation += p.Spin * seconds;
        }
    }

    private void Stop()
    {
        _timer?.Stop();
        _timer = null;
        Invalidate();
    }

    private sealed class Particle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Rotation { get; set; }
        public float Spin { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; } = Colors.White;
    }
}
