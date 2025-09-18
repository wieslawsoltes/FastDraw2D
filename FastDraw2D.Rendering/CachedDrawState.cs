// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Skia;
using SkiaSharp;

namespace FastDraw2D.Rendering;

public class CachedDrawState
{
    private readonly Action<SKCanvas, Rect, double> _draw;
	private Rect _bounds;
	private SKPicture? _picture;
    private SKMatrix _matrix;
    private double? _cachedZoom;

    public CachedDrawState(Rect bounds, Action<SKCanvas, Rect, double> draw)
	{
		_bounds = bounds;
        _draw = draw;
        _matrix = SKMatrix.Identity;
	}

    public SKMatrix Transform => _matrix;

    public void Invalidate(Rect bounds)
    {
        _bounds = bounds;
        _picture?.Dispose();
        _picture = null;
        _cachedZoom = null;
    }

    public void SetTransform(SKMatrix matrix)
    {
        _matrix = matrix;
    }

	public void Render(DrawingContext context, double zoom)
	{
		var custom = new CachedDrawDrawOperation(_bounds, Draw, zoom);

		context.Custom(custom);
	}

    private void Draw(ISkiaSharpApiLease skia, Rect bounds, double zoom)
	{
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var requiresRedraw =
            _picture is null ||
            !_cachedZoom.HasValue ||
            Math.Abs(_cachedZoom.Value - zoom) > 1e-6;

        if (requiresRedraw)
        {
            Record(bounds, zoom);

            if (_picture is null)
            {
                return;
            }
            _cachedZoom = zoom;
        }

        if (_picture is null)
        {
            return;
        }

        skia.SkCanvas.Save();
        var transform = _matrix;
        skia.SkCanvas.Concat(ref transform);
        skia.SkCanvas.DrawPicture(_picture);
        skia.SkCanvas.Restore();
	}

    private void Record(Rect bounds, double zoom)
	{
		var recorder = new SKPictureRecorder();
		var rect = new SKRect(0f, 0f, (float)bounds.Width, (float)bounds.Height);
		var canvas = recorder.BeginRecording(rect);
		_draw(canvas, bounds, zoom);
		_picture?.Dispose();
		_picture = recorder.EndRecording();
	}
}
