using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Skia;
using SkiaSharp;

namespace FastDraw2D.Rendering;

public class CachedDrawState
{
    private readonly Action<SKCanvas, Rect> _draw;
	private SKSurface? _surface;
	private Rect _bounds;
	private SKPicture? _picture;
    private SKMatrix _matrix;

    public CachedDrawState(Rect bounds, Action<SKCanvas, Rect> draw)
	{
		_bounds = bounds;
        _draw = draw;
        _matrix = SKMatrix.Identity;
	}

    public SKMatrix Transform => _matrix;

    public void Invalidate(Rect bounds)
    {
        _bounds = bounds;
        _surface?.Dispose();
        _picture?.Dispose();
        _surface = null;
        _picture = null;
    }

    public void SetTransform(SKMatrix matrix)
    {
        _matrix = matrix;
    }

	public void Render(DrawingContext context)
	{
		var custom = new CachedDrawDrawOperation(_bounds, Draw);

		context.Custom(custom);
	}

    private double _zoom = 0.0;

	private void Draw(ISkiaSharpApiLease skia, Rect bounds)
	{
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        if (_surface is null)
        {
            CreateSurface(skia, bounds);
        }

        if (_picture is null)
        {
            Record(bounds);
            _surface.Canvas.DrawPicture(_picture);
        }
        
        if (_zoom != _matrix.ScaleX)
        {
            _zoom = _matrix.ScaleX;
            //Record(bounds);
            //_surface.Canvas.Save();
            //_surface.Canvas.Translate(_matrix.TransX, _matrix.TransY);
            //_surface.Canvas.Scale(_matrix.ScaleX, _matrix.ScaleY);
            //_surface.Canvas.DrawPicture(_picture);
            //_surface.Canvas.Restore();
		}

		using var snapshot = _surface.Snapshot();

        skia.SkCanvas.Save();
        skia.SkCanvas.Translate(_matrix.TransX, _matrix.TransY);
        skia.SkCanvas.Scale(_matrix.ScaleX, _matrix.ScaleY);
		skia.SkCanvas.DrawImage(
            snapshot,
			new SKRect(0, 0, (float)bounds.Width, (float)bounds.Height),
			new SKRect(0, 0, (float)bounds.Width, (float)bounds.Height));
        skia.SkCanvas.Restore();
	}

    private void CreateSurface(ISkiaSharpApiLease skia, Rect bounds)
    {
        if (skia.GrContext is not null)
        {
            _surface = SKSurface.Create(
                skia.GrContext,
                false,
                new SKImageInfo((int)bounds.Width, (int)bounds.Width));
        }
        else
        {
            _surface = SKSurface.Create(
                new SKImageInfo(
                    (int)Math.Ceiling(bounds.Width),
                    (int)Math.Ceiling(bounds.Width),
                    SKImageInfo.PlatformColorType,
                    SKAlphaType.Premul));
        }
    }

    private void Record(Rect bounds)
	{
		var recorder = new SKPictureRecorder();
		var rect = new SKRect(0f, 0f, (float)bounds.Width, (float)bounds.Height);
		var canvas = recorder.BeginRecording(rect);
		_draw(canvas, bounds);
		_picture = recorder.EndRecording();
	}
}
