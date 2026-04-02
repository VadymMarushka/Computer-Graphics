using Lab_03_CG.MVVM.ViewModels;
using Lab_03_CG.Services;
using Lab_03_CG.Services.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ScottPlot;
using System;

namespace Lab_03_CG.MVVM.Views
{
    public sealed partial class GeometricView : Page
    {
        public GeometricViewModel VM { get; }

        private readonly IExportService _exportService;
        private bool _isDraggingCenter = false;
        private const float DragSnapRadiusPx = 14f;

        public GeometricView(GeometricViewModel viewModel, IExportService exportService)
        {
            InitializeComponent();
            VM = viewModel;
            _exportService = exportService;

            VM.Canvas.AddHandler(PointerPressedEvent,
                new PointerEventHandler(Canvas_PointerPressed), handledEventsToo: true);
            VM.Canvas.AddHandler(PointerMovedEvent,
                new PointerEventHandler(Canvas_PointerMoved), handledEventsToo: true);
            VM.Canvas.AddHandler(PointerReleasedEvent,
                new PointerEventHandler(Canvas_PointerReleased), handledEventsToo: true);

            VM.ExportRequested += ExportToPng;
        }

        private async void ExportToPng()
        {
            var window = (Application.Current as App)?.MainWindow
                         ?? throw new InvalidOperationException("Cannot find MainWindow.");

            string? path = await _exportService.PickSavePathAsync(window, "geometric_fractal");
            if (path is null) return;

            int w = (int)VM.Canvas.ActualWidth;
            int h = (int)VM.Canvas.ActualHeight;
            if (w == 0 || h == 0) { w = 1200; h = 1200; }

            VM.Canvas.Plot.SavePng(path, w, h);
        }

        private float PixelDistance(Pixel pointerPx, double plotX, double plotY)
        {
            var markerPx = VM.Canvas.Plot.GetPixel(new Coordinates(plotX, plotY));
            float dx = pointerPx.X - markerPx.X;
            float dy = pointerPx.Y - markerPx.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        private Pixel GetPixel(PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(VM.Canvas).Position;
            float scale = VM.Canvas.DisplayScale;
            return new Pixel((float)(pos.X * scale), (float)(pos.Y * scale));
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var px = GetPixel(e);
            if (PixelDistance(px, VM.CenterX, VM.CenterY) <= DragSnapRadiusPx)
            {
                _isDraggingCenter = true;
                VM.Canvas.UserInputProcessor.LeftClickDragPan(false);

                e.Handled = true;
                VM.Canvas.CapturePointer(e.Pointer);
            }
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDraggingCenter) return;
            var coords = VM.Canvas.Plot.GetCoordinates(GetPixel(e));
            VM.DragCenterTo(coords.X, coords.Y);

            e.Handled = true;
        }

        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDraggingCenter) return;

            _isDraggingCenter = false;
            VM.Canvas.UserInputProcessor.LeftClickDragPan(true);

            VM.Canvas.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }
    }
}