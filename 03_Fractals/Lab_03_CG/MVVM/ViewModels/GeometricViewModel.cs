using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab_03_CG.MVVM.Model;
using Lab_03_CG.Services.Abstractions;
using Lab_03_CG.Services.Implementaions;
using Microsoft.UI.Xaml.Media;
using ScottPlot;
using ScottPlot.WinUI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Globalization.NumberFormatting;
using Lab_03_CG;

namespace Lab_03_CG.MVVM.ViewModels
{
    public record Segment(double X1, double Y1, double X2, double Y2);

    public partial class GeometricViewModel : ObservableObject
    {
        [ObservableProperty] private DecimalFormatter _numberFormatter;

        private readonly IRainbowAnimationService _rainbowService;
        public SolidColorBrush RainbowBrush => _rainbowService.AnimatedBrush;
        public LinearGradientBrush RainbowGradientBrush => _rainbowService.AnimatedGradientBrush;

        public WinUIPlot Canvas { get; }

        private ScottPlot.Plottables.Scatter? _fractalScatter;
        private ScottPlot.Plottables.Marker? _centerMarker;

        [ObservableProperty] private int _selectedAxiomIndex = 0;
        [ObservableProperty] private int _selectedGeneratorIndex = 0;

        [ObservableProperty] private double _centerX = 0.0;
        [ObservableProperty] private double _centerY = 0.0;

        partial void OnCenterXChanged(double value) => SyncMarker();
        partial void OnCenterYChanged(double value) => SyncMarker();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IterationLabel))]
        private double _iterationCount = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IterationLabel))]
        private int _currentEvolutionIteration = 0;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IterationLabel))]
        private int _maxEvolutionIterations = 0;
        public int CalculateMaxIterations()
        {
            int axiomLinesCnt = SelectedAxiomIndex switch
            {
                0 => 1,
                1 => 3,
                2 => 4,
            };
            int generatorSegments = SelectedGeneratorIndex switch
            {
                0 => 4,
                1 => 8,
                2 => 8
            };
            int result;
            for (result = 1; result < 10; result++)
            {
                if (Math.Pow(generatorSegments, result) * axiomLinesCnt > 1024) return result - 1;
            }
            return result;
        }
        public string IterationLabel => $"Current Iteration: {CurrentEvolutionIteration} / {MaxEvolutionIterations}";

        [ObservableProperty] private int _selectedColorIndex = 0;

        private static readonly string[] _colorHex = { "Black", "Red", "Blue" };
        public string SelectedColorHex => _colorHex[SelectedColorIndex];
        partial void OnSelectedColorIndexChanged(int value) =>
            OnPropertyChanged(nameof(SelectedColorHex));

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RotationLabel))]
        private double _rotationAngle = 0.0;
        public string RotationLabel => $"Rotation Angle: {(int)RotationAngle}°";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotEvolving))]
        [NotifyCanExecuteChangedFor(nameof(BuildCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartEvolutionCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopEvolutionCommand))]
        private bool _isEvolving = false;
        public bool IsNotEvolving => !IsEvolving;

        [ObservableProperty]
        private double _axiomSize = 3.0;

        public GeometricViewModel(
            IRainbowAnimationService rainbowService,
            IFractalDataService fractalDataService,
            IDialogService dialogService)
        {
            _rainbowService = rainbowService;
            _fractalDataService = fractalDataService;
            _dialogService = dialogService;
            NumberFormatter = new DecimalFormatter { FractionDigits = 3 };
            _rainbowService.Start();

            Canvas = new WinUIPlot();
            Canvas.Plot.DataBackground.Color = ScottPlot.Colors.White;
            Canvas.Plot.Grid.LinePattern = LinePattern.Dashed;
            Canvas.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#00000033");
            Canvas.Plot.Axes.SetLimits(-5, 5, -5, 5);

            var xAxis = Canvas.Plot.Add.Line(-10000, 0, 10000, 0);
            xAxis.Color = ScottPlot.Color.FromHex("#00000077"); xAxis.LineWidth = 2;
            var yAxis = Canvas.Plot.Add.Line(0, -10000, 0, 10000);
            yAxis.Color = ScottPlot.Color.FromHex("#00000077"); yAxis.LineWidth = 2;

            Canvas.Plot.Axes.Rules.Clear();
            Canvas.Plot.Axes.Rules.Add(new ScottPlot.AxisRules.SquarePreserveX(
                Canvas.Plot.Axes.Bottom, Canvas.Plot.Axes.Left));
            Canvas.Plot.Axes.Rules.Add(new ScottPlot.AxisRules.MaximumBoundary(
                Canvas.Plot.Axes.Bottom, Canvas.Plot.Axes.Left,
                new ScottPlot.AxisLimits(-15, 15, -15, 15)));

            Canvas.UserInputProcessor.RightClickDragZoom(false);
            Canvas.Plot.Axes.Bottom.TickGenerator =
                new ScottPlot.TickGenerators.NumericAutomatic { TargetTickCount = 20 };
            Canvas.Plot.Axes.Left.TickGenerator =
                new ScottPlot.TickGenerators.NumericAutomatic { TargetTickCount = 20 };

            _centerMarker = Canvas.Plot.Add.Marker(CenterX, CenterY);
            _centerMarker.MarkerStyle.Shape = MarkerShape.FilledCircle;
            _centerMarker.MarkerStyle.Size = 12;
            _centerMarker.Color = ScottPlot.Color.FromHex("#E53935");
            _centerMarker.LegendText = "Generation Center";

            Canvas.Refresh();
        }

        private void SyncMarker()
        {
            if (_centerMarker is null) return;
            _centerMarker.X = CenterX;
            _centerMarker.Y = CenterY;
            Canvas.Refresh();
        }

        public void DragCenterTo(double x, double y)
        {
            CenterX = Math.Round(x, 3);
            CenterY = Math.Round(y, 3);
        }

        [RelayCommand(CanExecute = nameof(IsNotEvolving))]
        public void Build()
        {
            ClearEvolutionScatters();
            if (_fractalScatter != null)
            {
                Canvas.Plot.Remove(_fractalScatter);
                _fractalScatter = null;
            }

            // 1. Create axiom
            var segments = CreateAxiom();
            int iters = (int)IterationCount;

            // 2. Iterate generator over each segment
            for (int i = 0; i < iters; i++)
            {
                var next = new List<Segment>();
                foreach (var seg in segments)
                    next.AddRange(ApplyGenerator(seg));
                segments = next;
            }

            // 3. Draw
            DrawSegments(segments);
        }

        [RelayCommand]
        public void ClearCanvas()
        {
            StopEvolution();
            ClearEvolutionScatters();

            CenterX = 0; CenterY = 0;
            IterationCount = 0;

            if (_fractalScatter != null)
            {
                Canvas.Plot.Remove(_fractalScatter);
                _fractalScatter = null;
            }
            Canvas.Plot.Axes.SetLimits(-5, 5, -5, 5);
            Canvas.Refresh();
        }

        [RelayCommand]
        public void ResetView()
        {
            Canvas.Plot.Axes.SetLimits(-5, 5, -5, 5);
            Canvas.Refresh();
        }

        private CancellationTokenSource? _evolutionCts;

        private readonly List<ScottPlot.Plottables.Scatter> _evolutionScatters = new();

        private readonly Dictionary<Segment, ScottPlot.Plottables.Scatter>
            _segmentScatterMap = new();

        private readonly List<ScottPlot.Plottables.Scatter>
            _newIterationScatters = new();

        [RelayCommand(CanExecute = nameof(IsNotEvolving))]
        public async Task StartEvolution()
        {
            _evolutionCts?.Cancel();
            _evolutionCts = new CancellationTokenSource();
            var token = _evolutionCts.Token;

            IsEvolving = true;

            ClearEvolutionScatters();
            if (_fractalScatter != null)
            {
                Canvas.Plot.Remove(_fractalScatter);
                _fractalScatter = null;
            }

            _ = RunRainbowAnimationLoopAsync(token);

            try
            {
                CurrentEvolutionIteration = 0;
                MaxEvolutionIterations = CalculateMaxIterations();
                int maxIter = MaxEvolutionIterations;
                int[] segDelays = { 240, 30, 4, 1, 1 };
                var currentSegments = CreateAxiom();

                foreach (var seg in currentSegments)
                {
                    token.ThrowIfCancellationRequested();
                    AddSegmentScatter(seg, isNew: true);
                    await Task.Delay(segDelays[0], token);
                }

                CommitNewSegmentsToBlack();

                for (int iter = 1; iter <= maxIter; iter++)
                {
                    CurrentEvolutionIteration = iter;
                    token.ThrowIfCancellationRequested();
                    int delayMs = iter < segDelays.Length ? segDelays[iter] : segDelays[^1];
                    var nextSegments = new List<Segment>();

                    foreach (var old in currentSegments)
                    {
                        token.ThrowIfCancellationRequested();
                        var children = ApplyGenerator(old);

                        foreach (var child in children)
                        {
                            token.ThrowIfCancellationRequested();
                            AddSegmentScatter(child, isNew: true);
                            await Task.Delay(delayMs, token);
                        }
                        RemoveSegmentScatter(old);
                        nextSegments.AddRange(children);
                    }
                    CommitNewSegmentsToBlack();
                    currentSegments = nextSegments;
                }
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                IsEvolving = false;
            }
        }

        private async Task RunRainbowAnimationLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_newIterationScatters.Count > 0)
                    {
                        var currentRainbowColor = ToScottPlotColor(_rainbowService.AnimatedBrush.Color);

                        foreach (var sc in _newIterationScatters)
                        {
                            sc.Color = currentRainbowColor;
                        }
                        Canvas.Refresh();
                    }
                    await Task.Delay(33, token);
                }
            }
            catch (OperationCanceledException) { }
        }
        private static ScottPlot.Color ToScottPlotColor(Windows.UI.Color c) =>
            new ScottPlot.Color(c.R, c.G, c.B, c.A);

        private void AddSegmentScatter(Segment seg, bool isNew)
        {
            var xs = new double[] { seg.X1, seg.X2 };
            var ys = new double[] { seg.Y1, seg.Y2 };

            var scatter = Canvas.Plot.Add.Scatter(xs, ys);
            scatter.MarkerSize = 0;
            scatter.LineWidth = isNew ? 2.5f : 1.5f;

            scatter.Color = isNew
                ? ToScottPlotColor(_rainbowService.AnimatedBrush.Color)
                : ScottPlot.Color.FromHex("#000000");

            _segmentScatterMap[seg] = scatter;

            if (isNew)
                _newIterationScatters.Add(scatter);
            else
                _evolutionScatters.Add(scatter);

            Canvas.Refresh();
        }

        private void CommitNewSegmentsToBlack()
        {
            var blackColor = ScottPlot.Color.FromHex("#000000");

            foreach (var sc in _newIterationScatters)
            {
                sc.Color = blackColor;
                sc.LineWidth = 1.5f;
                _evolutionScatters.Add(sc);
            }

            _newIterationScatters.Clear();
            Canvas.Refresh();
        }

        [RelayCommand(CanExecute = nameof(IsEvolving))]
        public void StopEvolution()
        {
            _evolutionCts?.Cancel();
            _evolutionCts = null;
            IsEvolving = false;
        }

        [RelayCommand] public void ExportAsPNG() => ExportRequested?.Invoke();

        public event Action? ExportRequested;

        [RelayCommand]
        public async Task SaveToGallery(object parameter)
        {
            if (parameter is not Microsoft.UI.Xaml.FrameworkElement { XamlRoot: not null } element) return;
            byte[] imgBytes = Canvas.Plot.GetImageBytes(800, 600, ScottPlot.ImageFormat.Png);
            var dto = new AddGeometricFractalDTO
            {
                Image = imgBytes,
                SuggestedTitle = "Geometric Fractal",
                Initializer = SelectedAxiomIndex switch
                {
                    0 => "Line",
                    1 => "Triangle",
                    _ => "Square",
                },
                Generator = SelectedGeneratorIndex switch
                {
                    0 => "Koch Curve",
                    1 => "Double Koch",
                    _ => "Minkowski",
                },
                CenterX = CenterX,
                CenterY = CenterY,
                SideLength = AxiomSize,
                RotationAngle = (int)RotationAngle,
                LineColor = SelectedColorHex,
            };

            var fractal = await _dialogService.ShowAddFractalDialogAsync(element.XamlRoot, dto);

            if (fractal is not null)
            {
                await _fractalDataService.AddFractalAsync(fractal, dto.Image);
            }
        }

        private readonly IFractalDataService _fractalDataService;
        private readonly IDialogService _dialogService;

        private void RemoveSegmentScatter(Segment seg)
        {
            if (!_segmentScatterMap.TryGetValue(seg, out var sc)) return;
            Canvas.Plot.Remove(sc);
            _evolutionScatters.Remove(sc);
            _segmentScatterMap.Remove(seg);
            Canvas.Refresh();
        }

        private void ClearEvolutionScatters()
        {
            foreach (var sc in _evolutionScatters)
                Canvas.Plot.Remove(sc);
            foreach (var sc in _newIterationScatters)
                Canvas.Plot.Remove(sc);

            _evolutionScatters.Clear();
            _newIterationScatters.Clear();
            _segmentScatterMap.Clear();
            Canvas.Refresh();
        }


        private static (double x, double y) Rotate(
            double px, double py,
            double cx, double cy,
            double angleDeg)
        {
            double rad = angleDeg * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double dx = px - cx;
            double dy = py - cy;
            return (cx + dx * cos - dy * sin,
                    cy + dx * sin + dy * cos);
        }

        private Segment RotateSeg(Segment s)
        {
            var (x1, y1) = Rotate(s.X1, s.Y1, CenterX, CenterY, RotationAngle);
            var (x2, y2) = Rotate(s.X2, s.Y2, CenterX, CenterY, RotationAngle);
            return new Segment(x1, y1, x2, y2);
        }

        private List<Segment> CreateAxiom()
        {
            var raw = SelectedAxiomIndex switch
            {
                1 => CreateTriangle(),
                2 => CreateSquare(),
                _ => CreateLine(),
            };

            var result = new List<Segment>(raw.Count);
            foreach (var s in raw)
                result.Add(RotateSeg(s));
            return result;
        }

        private List<Segment> CreateLine() => new()
        {
            new Segment(CenterX - AxiomSize, CenterY,
                        CenterX + AxiomSize, CenterY)
        };

        private List<Segment> CreateTriangle()
        {
            double cx = CenterX, cy = CenterY;

            double r = AxiomSize;
            double tx = cx + r * Math.Cos(Math.PI / 2.0);
            double ty = cy + r * Math.Sin(Math.PI / 2.0);

            double blx = cx + r * Math.Cos(Math.PI / 2.0 + 2.0 * Math.PI / 3.0);
            double bly = cy + r * Math.Sin(Math.PI / 2.0 + 2.0 * Math.PI / 3.0);

            double brx = cx + r * Math.Cos(Math.PI / 2.0 - 2.0 * Math.PI / 3.0);
            double bry = cy + r * Math.Sin(Math.PI / 2.0 - 2.0 * Math.PI / 3.0);

            return new()
            {
                new Segment(tx,  ty,  brx, bry),
                new Segment(brx, bry, blx, bly),
                new Segment(blx, bly, tx,  ty),
            };
        }

        private List<Segment> CreateSquare()
        {
            double cx = CenterX, cy = CenterY, s = AxiomSize;
            double x0 = cx - s, y0 = cy + s;
            double x1 = cx + s, y1 = cy + s;
            double x2 = cx + s, y2 = cy - s;
            double x3 = cx - s, y3 = cy - s;

            return new()
            {
                new Segment(x0, y0, x1, y1),
                new Segment(x1, y1, x2, y2),
                new Segment(x2, y2, x3, y3),
                new Segment(x3, y3, x0, y0),
            };
        }

        private List<Segment> ApplyGenerator(Segment s) =>
            SelectedGeneratorIndex switch
            {
                0 => ApplyKoch(s),
                1 => ApplyDoubleKoch(s),
                2 => ApplyMinkowski(s),
            };

        private static List<Segment> ApplyKoch(Segment s)
        {
            double dx = s.X2 - s.X1;
            double dy = s.Y2 - s.Y1;

            double p1x = s.X1 + dx / 3.0;
            double p1y = s.Y1 + dy / 3.0;
            double p2x = s.X1 + dx * 2.0 / 3.0;
            double p2y = s.Y1 + dy * 2.0 / 3.0;

            double ex = p2x - p1x;
            double ey = p2y - p1y;
            double cos = Math.Cos(Math.PI / 3.0);
            double sin = Math.Sin(Math.PI / 3.0);
            double pkx = p1x + ex * cos - ey * sin;
            double pky = p1y + ex * sin + ey * cos;

            return new()
            {
                new Segment(s.X1, s.Y1, p1x,  p1y),
                new Segment(p1x,  p1y,  pkx,  pky),
                new Segment(pkx,  pky,  p2x,  p2y),
                new Segment(p2x,  p2y,  s.X2, s.Y2),
            };
        }
        private static List<Segment> ApplyDoubleKoch(Segment s)
        {
            double dx = s.X2 - s.X1;
            double dy = s.Y2 - s.Y1;

            double p1x = s.X1 + dx / 3.0;
            double p1y = s.Y1 + dy / 3.0;
            double p2x = s.X1 + dx * 2.0 / 3.0;
            double p2y = s.Y1 + dy * 2.0 / 3.0;

            double ex = p2x - p1x;
            double ey = p2y - p1y;
            double cost = Math.Cos(Math.PI / 3.0);
            double sint = Math.Sin(Math.PI / 3.0);
            double ptx = p1x + ex * cost - ey * sint;
            double pty = p1y + ex * sint + ey * cost;

            double cosb = Math.Cos(-Math.PI / 3.0);
            double sinb = Math.Sin(-Math.PI / 3.0);
            double pbx = p1x + ex * cosb - ey * sinb;
            double pby = p1y + ex * sinb + ey * cosb;

            return new()
            {
                new Segment(s.X1, s.Y1, p1x,  p1y),

                new Segment(p1x,  p1y,  ptx,  pty),
                new Segment(ptx,  pty,  p2x,  p2y),

                new Segment(p1x,  p1y,  pbx,  pby),
                new Segment(pbx,  pby,  p2x,  p2y),

                new Segment(p2x,  p2y,  s.X2, s.Y2),
            };
        }

        private static List<Segment> ApplyMinkowski(Segment s)
        {
            double dx = s.X2 - s.X1;
            double dy = s.Y2 - s.Y1;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len == 0) return new() { s };

            double ux = dx / len;
            double uy = dy / len;
            double nx = -uy;
            double ny = ux;

            double step = len / 4.0;

            double ax = s.X1, ay = s.Y1;

            (double x, double y) P(double along, double perp) =>
                (ax + ux * along * step + nx * perp * step,
                 ay + uy * along * step + ny * perp * step);

            var (p1x, p1y) = P(1, 0);
            var (p2x, p2y) = P(1, 1);
            var (p3x, p3y) = P(2, 1);
            var (p4x, p4y) = P(2, 0);
            var (p5x, p5y) = P(2, -1);
            var (p6x, p6y) = P(3, -1);
            var (p7x, p7y) = P(3, 0);

            return new()
            {
                new Segment(ax,  ay,  p1x, p1y),
                new Segment(p1x, p1y, p2x, p2y),
                new Segment(p2x, p2y, p3x, p3y),
                new Segment(p3x, p3y, p4x, p4y),
                new Segment(p4x, p4y, p5x, p5y),
                new Segment(p5x, p5y, p6x, p6y),
                new Segment(p6x, p6y, p7x, p7y),
                new Segment(p7x, p7y, s.X2, s.Y2),
            };
        }


        private void DrawSegments(List<Segment> segments)
        {
            if (_fractalScatter != null)
            {
                Canvas.Plot.Remove(_fractalScatter);
                _fractalScatter = null;
            }

            if (segments.Count == 0) { Canvas.Refresh(); return; }

            int n = segments.Count;
            var xs = new double[n * 3];
            var ys = new double[n * 3];

            for (int i = 0; i < n; i++)
            {
                int b = i * 3;
                xs[b] = segments[i].X1; ys[b] = segments[i].Y1;
                xs[b + 1] = segments[i].X2; ys[b + 1] = segments[i].Y2;
                xs[b + 2] = double.NaN; ys[b + 2] = double.NaN;
            }

            _fractalScatter = Canvas.Plot.Add.Scatter(xs, ys);
            _fractalScatter.MarkerSize = 0;
            _fractalScatter.LineWidth = 1.5f;
            string hexCode = SelectedColorHex switch
            {
                "Black" => "#000000",
                "Red" => "#FF0000",
                "Blue" => "#0000FF",
                _ => "#000000"
            };
            _fractalScatter.Color = ScottPlot.Color.FromHex(hexCode);

            Canvas.Refresh();
        }
    }
}