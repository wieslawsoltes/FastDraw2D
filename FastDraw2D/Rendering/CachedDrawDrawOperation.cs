// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;

namespace FastDraw2D.Rendering;

public class CachedDrawDrawOperation : ICustomDrawOperation
{
	private readonly Action<ISkiaSharpApiLease, Rect> _draw;

	public CachedDrawDrawOperation(Rect bounds, Action<ISkiaSharpApiLease, Rect> draw)
	{
		_draw = draw;
		Bounds = new Rect(0, 0, bounds.Width, bounds.Height);
	}

	public Rect Bounds { get; }

	void IDisposable.Dispose()
	{
		// nothing to do.
	}

	bool ICustomDrawOperation.HitTest(Point p) => Bounds.Contains(p);

	bool IEquatable<ICustomDrawOperation>.Equals(ICustomDrawOperation? other) => false;

	void ICustomDrawOperation.Render(ImmediateDrawingContext context)
	{
		using var skia = context.TryGetFeature<ISkiaSharpApiLeaseFeature>()?.Lease();
		if (skia is null)
		{
			return;
		}
		_draw(skia, Bounds);
	}
}
