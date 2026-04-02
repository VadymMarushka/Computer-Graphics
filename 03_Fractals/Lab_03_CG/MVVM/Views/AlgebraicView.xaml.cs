using ComputeSharp.D2D1;
using ComputeSharp.D2D1.WinUI;
using Lab_03_CG.MVVM.ViewModels;
using Lab_03_CG.Services;
using Lab_03_CG.Services.Abstractions;
using Lab_03_CG.Shaders;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lab_03_CG.MVVM.Views
{
    public sealed partial class AlgebraicView : Page
    {
        public AlgebraicViewModel VM { get; }

        private readonly IExportService _exportService;
        private PixelShaderEffect<FractalShader> _fractalEffect;
        private CanvasDevice? _canvasDevice;

        private bool _isPanning;
        private Point _panStartPos;
        private double _panStartX, _panStartY;
        private const double ZoomStep = 1.15;

        public AlgebraicView(AlgebraicViewModel viewModel, IExportService exportService)
        {
            InitializeComponent();
            VM = viewModel;
            VM.CaptureImageBytesAsync = async () => await GetCanvasBytes();
            _exportService = exportService;

            VM.ExportRequested += ExportToPng;
        }

        private async Task<byte[]> GetCanvasBytes()
        {
            FractalCanvas.Paused = true;

            int w = (int)FractalCanvas.ActualWidth;
            int h = (int)FractalCanvas.ActualHeight;
            using var rt = new CanvasRenderTarget(_canvasDevice, w, h, 96);
            using (var ds = rt.CreateDrawingSession())
            {
                ds.DrawImage(_fractalEffect);
            }
            using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            await rt.SaveAsync(stream, CanvasBitmapFileFormat.Png);
            var bytes = new byte[stream.Size];
            await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, Windows.Storage.Streams.InputStreamOptions.None);

            FractalCanvas.Paused = false;
            return bytes;
        }

        private void FractalCanvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            _fractalEffect = new PixelShaderEffect<FractalShader>();
            _canvasDevice = sender.Device;
        }

        private void FractalCanvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            UpdateShaderBuffer(
                (float)sender.Size.Width,
                (float)sender.Size.Height,
                (float)args.Timing.TotalTime.TotalSeconds);

            args.DrawingSession.DrawImage(_fractalEffect);
        }

        private void UpdateShaderBuffer(float w, float h, float time)
        {
            _fractalEffect.ConstantBuffer = new FractalShader(
                w, h, time,
                (float)VM.ConstantReal,
                (float)VM.ConstantImaginary,
                (float)VM.MaxIterations,
                (float)VM.EscapeRadius,
                (float)VM.SelectedFormulaIndex,
                (float)VM.SelectedPaletteIndex,
                (float)VM.SelectedModeIndex,
                (float)VM.Z0Real,
                (float)VM.Z0Imaginary,
                (float)VM.PanX,
                (float)VM.PanY,
                (float)VM.Zoom,
                VM.ChromaticMode
            );
        }

        private async void ExportToPng()
        {
            if (_canvasDevice is null || _fractalEffect is null) return;

            var window = (Application.Current as App)?.MainWindow
                         ?? throw new InvalidOperationException("Cannot find MainWindow.");

            string? path = await _exportService.PickSavePathAsync(window, "algebraic_fractal");
            if (path is null) return;

            int w = (int)FractalCanvas.ActualWidth;
            int h = (int)FractalCanvas.ActualHeight;
            if (w == 0 || h == 0) return;

            using var rt = new CanvasRenderTarget(_canvasDevice, w, h, 96);
            using (var ds = rt.CreateDrawingSession())
            {
                ds.Clear(Microsoft.UI.Colors.Black);
                UpdateShaderBuffer(w, h, time: 0f);
                ds.DrawImage(_fractalEffect);
            }

            await rt.SaveAsync(path, CanvasBitmapFileFormat.Png);
        }

        private void FractalCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isPanning = true;
            _panStartPos = e.GetCurrentPoint(FractalCanvas).Position;
            _panStartX = VM.PanX;
            _panStartY = VM.PanY;
            FractalCanvas.CapturePointer(e.Pointer);
            e.Handled = true;
        }

        private void FractalCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isPanning) return;

            var pos = e.GetCurrentPoint(FractalCanvas).Position;
            double w = FractalCanvas.ActualWidth;
            double h = FractalCanvas.ActualHeight;
            if (w == 0 || h == 0) return;

            double dx = (pos.X - _panStartPos.X) / (w * 0.5) * (w / h) * VM.Zoom;
            double dy = (pos.Y - _panStartPos.Y) / (h * 0.5) * VM.Zoom;

            VM.PanX = _panStartX - dx;
            VM.PanY = _panStartY - dy;
            e.Handled = true;
        }

        private void FractalCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isPanning = false;
            FractalCanvas.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }

        private void FractalCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            int delta = e.GetCurrentPoint(FractalCanvas).Properties.MouseWheelDelta;
            double factor = delta > 0 ? 1.0 / ZoomStep : ZoomStep;
            VM.Zoom = Math.Clamp(VM.Zoom * factor, 1e-6, 10.0);
            e.Handled = true;
        }
    }
}