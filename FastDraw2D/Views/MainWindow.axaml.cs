// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Rendering;

namespace FastDraw2D.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        RendererDiagnostics.DebugOverlays = RendererDebugOverlays.Fps 
                                            | RendererDebugOverlays.LayoutTimeGraph 
                                            | RendererDebugOverlays.RenderTimeGraph;
#endif
    }
}
