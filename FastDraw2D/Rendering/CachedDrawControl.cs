// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using FastDraw2D.Rendering.Nodes;
using SkiaSharp;

namespace FastDraw2D.Rendering;

public class CachedDrawControl : TemplatedControl
{
	private readonly CachedDrawState _state;
    private List<DrawNode>? _nodes;
    private Point _start;
    private bool _pressed;
    private Point _diff;

    private HashSet<double> _zoomStates = new();
    private const double _baseZoomFactor = 1.15;
    private const int _minZoomLevel = -20;
    private const int _maxZoomLevel = 40;
    private int _currentZoomLevel = 0;

    public CachedDrawControl()
	{
		_state = new CachedDrawState(Bounds, Draw);
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		_state.Render(context, _state.Transform.ScaleX);
	}

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty)
        {
            var bounds = change.GetNewValue<Rect>();

            CreateNodes(bounds);
            
            _diff = new Point();
            _state.SetTransform(SKMatrix.Identity);
            _state.Invalidate(bounds);
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _start = e.GetPosition(this);
        _pressed = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        _pressed = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_pressed)
        {
            var position = e.GetPosition(this);

            var delta = position - _start;
            _start = position;
            var transform = _state.Transform;
            var matrix = SKMatrix.CreateTranslation((float)delta.X, (float)delta.Y);
            transform = transform.PostConcat(matrix);
            _state.SetTransform(transform);

            InvalidateVisual();
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var position = e.GetPosition(this);

        int zoomDelta = e.Delta.Y > 0 ? 1 : -1;
        int newZoomLevel = Math.Clamp(_currentZoomLevel + zoomDelta, _minZoomLevel, _maxZoomLevel);

        if (newZoomLevel == _currentZoomLevel)
            return; // Reached zoom limit, no further action

        _currentZoomLevel = newZoomLevel;

        double zoomFactor = Math.Pow(_baseZoomFactor, _currentZoomLevel);

        // Reset and build transform deterministically from the zoom factor
        var transform = SKMatrix.CreateIdentity();

        // Centered scaling around cursor position
        transform = transform.PostConcat(SKMatrix.CreateTranslation((float)-position.X, (float)-position.Y));
        transform = transform.PostConcat(SKMatrix.CreateScale((float)zoomFactor, (float)zoomFactor));
        transform = transform.PostConcat(SKMatrix.CreateTranslation((float)position.X, (float)position.Y));
        
        _zoomStates.Add(transform.ScaleX);
        Console.WriteLine($"Zoom Level: {_currentZoomLevel}, Zoom Factor: {zoomFactor:F4}, States count: {_zoomStates.Count}");

        _state.SetTransform(transform);
        
        InvalidateVisual();
    }

    private void CreateNodes(Rect bounds)
    {
        var random = new Random();
        var maxX = (long)bounds.Width;
        var maxY = (long)bounds.Height;

        if (_nodes is null)
        {
            _nodes = new List<DrawNode>();

            for (var i = 0; i < 10_000; i++)
            {
                var node = new PathDrawNode();
                _nodes.Add(node);
            }
        }
        else
        {
            foreach (var node in _nodes)
            {
                node.Reset();
            }
        }

        foreach (var node in _nodes)
        {
            if (node is PathDrawNode pathNode)
            {
                var x = random.NextInt64(0, maxX);
                var y = random.NextInt64(0, maxY);
                var width = random.NextInt64(0, 30);
                var height = random.NextInt64(0, 30);
                var rect = SKRect.Create(x, y, width, height);
                pathNode.AddRect(rect);
            }
        }
    }

    private void Draw(SKCanvas canvas, Rect bounds, double zoom)
    {
        canvas.Save();
        canvas.Clear(SKColors.White);

        if (_nodes is not null)
        {
            foreach (var node in _nodes)
            {
                node.Draw(canvas, zoom);
            }
        }

        canvas.Restore();
    }
}
