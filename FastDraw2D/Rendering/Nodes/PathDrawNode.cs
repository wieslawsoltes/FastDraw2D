// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using SkiaSharp;

namespace FastDraw2D.Rendering.Nodes;

public class PathDrawNode : DrawNode
{
    private SKPath _path;
    private SKPaint _paint;

    public PathDrawNode()
    {
        _path = new SKPath();
        _paint = new SKPaint();
        _paint.Color = SKColors.Black;
        _paint.IsAntialias = false;
        _paint.Style = SKPaintStyle.Stroke;
        _paint.StrokeWidth = 2;
    }

    public override void Reset()
    {
        _path.Reset();
    }

    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawPath(_path, _paint);
    }

    public void AddRect(SKRect rect)
    {
        _path.AddRect(rect);
    }
}
