// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using SkiaSharp;

namespace FastDraw2D.Rendering.Nodes;

public class PathDrawNode : DrawNode
{
    private readonly SKPath _path;
    private readonly SKPaint _paint;
    private readonly float _baseStrokeWidth;

    public PathDrawNode(SKPath path, SKPaint paint)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(paint);

        _path = path;
        _paint = paint;
        _baseStrokeWidth = paint.StrokeWidth;
    }

    public override void Reset()
    {
        _path.Reset();
    }

    public override void Draw(SKCanvas canvas, double zoom)
    {
        var stroke = _baseStrokeWidth > 0 ? _baseStrokeWidth : 2f;
        if (Math.Abs(zoom) > double.Epsilon)
        {
            _paint.StrokeWidth = stroke / (float)zoom;
        }
        else
        {
            _paint.StrokeWidth = stroke;
        }
        canvas.DrawPath(_path, _paint);
    }

    public void AddRect(SKRect rect)
    {
        _path.AddRect(rect);
    }
}
