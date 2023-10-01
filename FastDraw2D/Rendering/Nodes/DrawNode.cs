using SkiaSharp;

namespace FastDraw2D.Rendering.Nodes;

public abstract class DrawNode
{
    public abstract void Draw(SKCanvas canvas);
    public abstract void Reset();
}
