using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab_02_CG.Domain;
using Lab_02_CG.Services.Abstractions;
using Lab_02_CG.Services.Models;
using ScottPlot;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Lab_02_CG.MVVM.ViewModels
{
    public partial class ExperimentsViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _intervalsCount;

        [ObservableProperty]
        private List<int> _intervalsCntList;

        [ObservableProperty]
        private int _pointsCount;

        [ObservableProperty]
        private List<int> _pointsCntList;

        public WpfPlot PerformanceCanvas { get; }
        public WpfPlot StabilityCanvas { get; }

        [ObservableProperty]
        private DataTable _result1Table;

        [ObservableProperty]
        private DataTable _result2Table;

        private readonly IBezierService _bezierService;
        public ExperimentsViewModel(IBezierService bezierService)
        {
            _bezierService = bezierService;
            IntervalsCount = 10;
            IntervalsCntList = new List<int>() { 10, 20, 40, 60, 100, 125, 150, 200 , 250 , 300, 400, 500, 750 };
            PointsCount = 3;
            PointsCntList = new List<int>() { 3, 5, 10 , 15 , 20 , 25 , 30 , 35, 40 , 45, 50, 100,250,500, 1001, 1050};
            PerformanceCanvas = new WpfPlot();
            StabilityCanvas = new WpfPlot();

            Result1Table = new DataTable();
            Result1Table.Columns.Add("Intervals count", typeof(int));
            Result1Table.Columns.Add("Parametric time", typeof(string));
            Result1Table.Columns.Add("Matrix time", typeof(string));


            Result2Table = new DataTable();
            Result2Table.Columns.Add("Points N", typeof(int));
            Result2Table.Columns.Add("Parametric error", typeof(string));
            Result2Table.Columns.Add("Matrix error", typeof(string));
            Result2Table.Columns.Add("Bernstein Sum (t = 0.5)", typeof(string));
            
            ConfigurePerformanceCanvas(PerformanceCanvas);
            ConfigureStabilityCanvas(StabilityCanvas);
        }

        private void ApplyBaseSettings(WpfPlot canvas)
        {
            canvas.Plot.DataBackground.Color = ScottPlot.Colors.White;
            canvas.Plot.FigureBackground.Color = ScottPlot.Colors.White;
            canvas.Plot.Grid.LinePattern = LinePattern.Dashed;
            canvas.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#00000033");

            var tickerX = new ScottPlot.TickGenerators.NumericAutomatic { TargetTickCount = 20 };
            canvas.Plot.Axes.Bottom.TickGenerator = tickerX;
            var tickerY = new ScottPlot.TickGenerators.NumericAutomatic { TargetTickCount = 20 };
            canvas.Plot.Axes.Left.TickGenerator = tickerY;
            canvas.UserInputProcessor.Disable();
        }

        private void ConfigurePerformanceCanvas(WpfPlot canvas)
        {
            ApplyBaseSettings(canvas);

            canvas.Plot.Axes.Bottom.Label.Text = "Number of Intervals";
            canvas.Plot.Axes.Bottom.Label.ForeColor = ScottPlot.Colors.Black;
            canvas.Plot.Axes.Bottom.Label.FontSize = 14;
            canvas.Plot.Axes.Bottom.Label.Bold = false;

            canvas.Plot.Axes.Left.Label.Text = "Execution Time (ms)";
            canvas.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.Black;
            canvas.Plot.Axes.Left.Label.FontSize = 14;
            canvas.Plot.Axes.Left.Label.Bold = false;

            canvas.Plot.ShowLegend(ScottPlot.Alignment.UpperLeft, ScottPlot.Orientation.Horizontal);

            canvas.Refresh();
        }

        private void ConfigureStabilityCanvas(WpfPlot canvas)
        {
            ApplyBaseSettings(canvas);

            canvas.Plot.Axes.Title.Label.ForeColor = ScottPlot.Colors.Black;
            canvas.Plot.Axes.Title.Label.FontSize = 18;
            canvas.Plot.Axes.Title.Label.Bold = true;

            canvas.Plot.ShowLegend(ScottPlot.Alignment.UpperLeft);

            canvas.Plot.Axes.Rules.Clear();
            var squareRule = new ScottPlot.AxisRules.SquarePreserveX(canvas.Plot.Axes.Bottom, canvas.Plot.Axes.Left);
            canvas.Plot.Axes.Rules.Add(squareRule);

            ScottPlot.Plottables.FloatingAxis floatingX = new(canvas.Plot.Axes.Bottom);
            ScottPlot.Plottables.FloatingAxis floatingY = new(canvas.Plot.Axes.Left);

            canvas.Plot.Axes.Frameless();
            canvas.Plot.Add.Plottable(floatingX);
            canvas.Plot.Add.Plottable(floatingY);

            canvas.Refresh();
        }

        [RelayCommand]
        public async Task StartExperiment1()
        {
            PerformanceCanvas.Plot.Clear();
            ConfigurePerformanceCanvas(PerformanceCanvas);

            List<ControlPoint> cps = GenerateRandomPoints(3);
            _bezierService.CalculateParametric(cps, 0.01);
            _bezierService.CalculateMatrix(cps, 0.01);

            List<double> xs = new List<double>();
            List<double> paramYs = new List<double>();
            List<double> matrixYs = new List<double>();

            for (int i = 0; i < IntervalsCntList.Count; i++)
            {
                IntervalsCount = IntervalsCntList[i];

                var times = await Task.Run(() => Experiment1Iteration(IntervalsCount));

                xs.Add(IntervalsCount);
                paramYs.Add(times.ParametricTime);
                matrixYs.Add(times.MatrixTime);

                Result1Table.Rows.Add(
                    IntervalsCount,
                    Math.Round(times.ParametricTime, 4).ToString("F4") + "ms",
                    Math.Round(times.MatrixTime, 4).ToString("F4") + "ms"
                );

                PerformanceCanvas.Plot.Clear();
                ConfigurePerformanceCanvas(PerformanceCanvas);

                var sp1 = PerformanceCanvas.Plot.Add.Scatter(xs.ToArray(), paramYs.ToArray());
                sp1.LegendText = "Parametric";
                sp1.Color = ScottPlot.Colors.Blue;
                sp1.LineWidth = 2;
                sp1.MarkerSize =5;

                var sp2 = PerformanceCanvas.Plot.Add.Scatter(xs.ToArray(), matrixYs.ToArray());
                sp2.LegendText = "Matrix";
                sp2.Color = ScottPlot.Colors.Red;
                sp2.LineWidth = 2;
                sp2.MarkerSize = 5;

                PerformanceCanvas.Plot.Axes.AutoScale();
                PerformanceCanvas.Refresh();

                await Task.Delay(500);
            }
        }
        private (double ParametricTime, double MatrixTime) Experiment1Iteration(int intervalsCnt)
        {
            const int iterations = 1000;

            Stopwatch sw = new Stopwatch();
            List<ControlPoint> cps = GenerateRandomPoints(3);
            double dT = 1.0 / intervalsCnt;

            _bezierService.CalculateParametric(cps, dT);
            _bezierService.CalculateMatrix(cps, dT);

            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                _bezierService.CalculateParametric(cps, dT);
            }
            sw.Stop();
            double paramTime = sw.Elapsed.TotalMilliseconds / iterations;

            sw.Reset();
            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                _bezierService.CalculateMatrix(cps, dT);
            }
            sw.Stop();
            double matrixTime = sw.Elapsed.TotalMilliseconds / iterations;

            return (paramTime, matrixTime);
        }
        [RelayCommand]
        public async Task StartExperiment2()
        {
            Result2Table.Rows.Clear();
            var cachedResults = new List<(string ParamErrStr, string BernsteinSumStr, string MatrixErrStr, List<ControlPoint> Cps, double[] ParamX, double[] ParamY, double[] MatrixX, double[] MatrixY)>();

            int i = 0;
            while (i < PointsCntList.Count)
            {
                PointsCount = PointsCntList[i];

                if (cachedResults.Count <= i)
                {
                    var res = await Task.Run(() => Experiment2Iteration(PointsCount));
                    cachedResults.Add(res);
                }

                var currentRes = cachedResults[i];

                Result2Table.Rows.Clear();
                for (int j = 0; j <= i; j++)
                {
                    Result2Table.Rows.Add(
                        PointsCntList[j],
                        cachedResults[j].ParamErrStr,
                        cachedResults[j].MatrixErrStr,
                        cachedResults[j].BernsteinSumStr
                    );
                }

                StabilityCanvas.Plot.Clear();
                ConfigureStabilityCanvas(StabilityCanvas);

                double[] cpX = new double[currentRes.Cps.Count];
                double[] cpY = new double[currentRes.Cps.Count];
                for (int j = 0; j < currentRes.Cps.Count; j++)
                {
                    cpX[j] = currentRes.Cps[j].X;
                    cpY[j] = currentRes.Cps[j].Y;
                }
                var cpPlot = StabilityCanvas.Plot.Add.Scatter(cpX, cpY);
                cpPlot.LegendText = "Control Points";
                cpPlot.Color = ScottPlot.Colors.Gray;
                cpPlot.LinePattern = LinePattern.Dashed;
                cpPlot.MarkerSize = 7;
                cpPlot.MarkerShape = MarkerShape.OpenCircle;

                double[] startAnchorX = new double[] { currentRes.Cps[0].X };
                double[] startAnchorY = new double[] { currentRes.Cps[0].Y };
                var startPlot = StabilityCanvas.Plot.Add.Scatter(startAnchorX, startAnchorY);
                startPlot.LegendText = "Start Anchor (P0)";
                startPlot.Color = ScottPlot.Colors.Purple;
                startPlot.LineWidth = 0;
                startPlot.MarkerSize = 10;
                startPlot.MarkerShape = MarkerShape.FilledCircle;

                double[] endAnchorX = new double[] { currentRes.Cps[currentRes.Cps.Count - 1].X };
                double[] endAnchorY = new double[] { currentRes.Cps[currentRes.Cps.Count - 1].Y };
                var endPlot = StabilityCanvas.Plot.Add.Scatter(endAnchorX, endAnchorY);
                endPlot.LegendText = "End Anchor (PN)";
                endPlot.Color = ScottPlot.Colors.Green;
                endPlot.LineWidth = 1;
                endPlot.MarkerSize = 10;
                endPlot.MarkerShape = MarkerShape.FilledCircle;

                var paramPlot = StabilityCanvas.Plot.Add.Scatter(currentRes.ParamX, currentRes.ParamY);
                paramPlot.LegendText = "Parametric Curve";
                paramPlot.Color = ScottPlot.Colors.Blue;
                paramPlot.LineWidth = 3;
                paramPlot.MarkerSize = 3;
                paramPlot.MarkerShape = MarkerShape.FilledCircle;
                paramPlot.MarkerColor = ScottPlot.Colors.Cyan;

                var matrixPlot = StabilityCanvas.Plot.Add.Scatter(currentRes.MatrixX, currentRes.MatrixY);
                matrixPlot.LegendText = "Matrix Curve";
                matrixPlot.Color = ScottPlot.Colors.Red;
                matrixPlot.LineWidth = 3;
                matrixPlot.MarkerSize = 0;
                matrixPlot.MarkerShape = MarkerShape.FilledCircle;
                matrixPlot.MarkerStyle.FillColor = ScottPlot.Colors.Yellow;

                StabilityCanvas.Plot.Axes.Title.Label.Text = $"Stability Test: {PointsCount} Points";
                StabilityCanvas.Refresh();

                _waitForStepTcs = new TaskCompletionSource<int>();
                int step = await _waitForStepTcs.Task;
                i += step;
                if (i < 0) i = 0;
            }
        }
        private (string ParamErrStr, string BernsteinSumStr, string MatrixErrStr, List<ControlPoint> Cps, double[] ParamX, double[] ParamY, double[] MatrixX, double[] MatrixY)
            Experiment2Iteration(int pointsCnt)
        {
            List<ControlPoint> cps = GenerateRandomPoints(pointsCnt);

            var res1 = _bezierService.CalculateParametric(cps, 1.0 / 750);
            var res2 = _bezierService.CalculateMatrix(cps, 1.0 / 750);

            ControlPoint exactLastPoint = cps[cps.Count - 1];

            var paramLastPoint = res1.Points[res1.Points.Length - 1];
            ParametricBezierPoint[] paramBezierPoints = (res1 as ParametricBezierResponse).Points;
            var matrixLastPoint = res2.Points[res2.Points.Length - 1];

            double paramErr = Math.Sqrt(Math.Pow(paramLastPoint.X - exactLastPoint.X, 2) + Math.Pow(paramLastPoint.Y - exactLastPoint.Y, 2));
            double matrixErr = Math.Sqrt(Math.Pow(matrixLastPoint.X - exactLastPoint.X, 2) + Math.Pow(matrixLastPoint.Y - exactLastPoint.Y, 2));

            string paramErrStr = (paramErr <= 1000) ? paramErr.ToString("F8") : " > 1000";
            string matrixErrStr = (matrixErr <= 1000) ? matrixErr.ToString("F8") : " > 1000";

            double[] paramX = new double[res1.Points.Length];
            double[] paramY = new double[res1.Points.Length];
            for (int i = 0; i < res1.Points.Length; i++)
            {
                paramX[i] = res1.Points[i].X;
                paramY[i] = res1.Points[i].Y;
            }

            double[] matrixX = new double[res2.Points.Length];
            double[] matrixY = new double[res2.Points.Length];
            for (int i = 0; i < res2.Points.Length; i++)
            {
                matrixX[i] = res2.Points[i].X;
                matrixY[i] = res2.Points[i].Y;
            }
            string BernsteinSumStr = ((res1 as ParametricBezierResponse).Points[res1.Points.Length / 2].BernsteinValues.Sum()).ToString();
            return (paramErrStr, BernsteinSumStr, matrixErrStr, cps, paramX, paramY, matrixX, matrixY);
        }
        private List<ControlPoint> GenerateRandomPoints(int count)
        {
            List<ControlPoint> randomPoints = new List<ControlPoint>(count);
            Random rnd = new Random();

            for (int i = 0; i < count; i++)
            {
                double randomX, randomY;
                do
                {
                    randomX = rnd.NextDouble() * 20.0 - 10.0;
                    randomY = rnd.NextDouble() * 20.0 - 10.0;
                } while (randomX < -5 && randomY > 5);

                randomPoints.Add(new ControlPoint($"P{i}", randomX, randomY));
            }

            return randomPoints;
        }

        private TaskCompletionSource<bool>? _waitForNextButtonTcs;

        private TaskCompletionSource<int>? _waitForStepTcs;

        [RelayCommand]
        public void NextExperimentStep()
        {
            _waitForStepTcs?.TrySetResult(1);
        }

        [RelayCommand]
        public void PreviousExperimentStep()
        {
            _waitForStepTcs?.TrySetResult(-1);
        }
    }
}
