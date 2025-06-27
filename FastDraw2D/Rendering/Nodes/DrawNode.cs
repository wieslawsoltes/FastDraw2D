// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using SkiaSharp;

namespace FastDraw2D.Rendering.Nodes;

public abstract class DrawNode
{
    public abstract void Draw(SKCanvas canvas);
    public abstract void Reset();
}
