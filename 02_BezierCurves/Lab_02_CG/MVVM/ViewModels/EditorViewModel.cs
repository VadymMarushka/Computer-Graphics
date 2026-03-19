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
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Lab_02_CG.MVVM.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        // Properties

        [ObservableProperty]
        private ObservableCollection<ControlPoint> _controlPoints;

        [ObservableProperty]
        private ObservableCollection<MatrixRowElement> _matrixRowElements;

        [ObservableProperty]
        private string _cursor;

        [ObservableProperty]
        private double _x_tb;

        [ObservableProperty]
        private double _y_tb;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveCommand))]
        private bool _isMoveMode;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        private bool _isAddMode;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DragCommand))]
        private bool _isDragMode;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private bool _isDeleteMode;

        [ObservableProperty]
        private List<string> _colorNames;

        [ObservableProperty]
        private HexColor _line_CB;
        [ObservableProperty]
        private HexColor _point_CB;
        [ObservableProperty]
        private HexColor _bezier_Parametric_CB;
        [ObservableProperty]
        private HexColor _bezier_Matrix_CB;

        [ObservableProperty]
        private int _intervalsParametric;
        [ObservableProperty]
        private int _intervalsMatrix;
        [ObservableProperty]
        private int _matrixRow;

        [ObservableProperty]
        private DataTable _parametricTable;
        [ObservableProperty]
        private DataTable _matrixTable;
        [ObservableProperty]
        private DataTable _tabulationTable;

        private double[][] _cachedMatrix;

        [ObservableProperty]
        private double _minT_TB;
        [ObservableProperty]
        private double _maxT_TB;
        [ObservableProperty]
        private double _stepT_TB;
        [ObservableProperty]
        private string _method_CB;
        public WpfPlot Canvas { get; set; }

        private ControlPoint _draggedPoint = null;

        private List<ScottPlot.Plottables.Marker> _plottedMarkers = new();
        private List<ScottPlot.Plottables.Text> _plottedLabels = new();
        private ScottPlot.Plottables.Scatter _plottedLine = null;
        private ScottPlot.Plottables.Scatter _parametricCurve = null;
        private ScottPlot.Plottables.Scatter _matrixCurve = null;

        private readonly IBezierService _bezierService;
        private readonly IPopUpService _popUpService;
        public EditorViewModel(IBezierService bezierSerivce, IPopUpService popUpService)
        {
            _bezierService = bezierSerivce;
            _popUpService = popUpService;
            ControlPoints = new ObservableCollection<ControlPoint>();
            ControlPoints.CollectionChanged += ControlPoints_CollectionChanged;

            ColorNames = HexColor.Colors.Keys.ToList();
            Line_CB = new HexColor();
            Point_CB = new HexColor("Green");
            Bezier_Parametric_CB = new HexColor("Blue");
            Bezier_Matrix_CB = new HexColor("Red");

            Line_CB.PropertyChanged += (sender, args) => RenderPoints();
            Point_CB.PropertyChanged += (sender, args) => RenderPoints();
            Bezier_Matrix_CB.PropertyChanged += (sender, args) => RenderPoints();
            Bezier_Parametric_CB.PropertyChanged += (sender, args) => RenderPoints();

            IntervalsParametric = IntervalsMatrix = 100;
            MatrixRow = 2;
            MatrixRowElements = new ObservableCollection<MatrixRowElement>();
            MinT_TB = 0;
            StepT_TB = 0.1;
            MaxT_TB = 1;
            Method_CB = "Parametric";

            // Canvas setup

            Canvas = new WpfPlot();

            Canvas.Plot.DataBackground.Color = ScottPlot.Colors.White;
            Canvas.Plot.Grid.LinePattern = LinePattern.Dashed;
            Canvas.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#00000033");

            Canvas.Plot.Axes.SetLimits(-10, 10, -10, 10);

            var xAxisLine = Canvas.Plot.Add.Line(-10000, 0, 10000, 0);
            xAxisLine.Color = ScottPlot.Color.FromHex("#00000077");
            xAxisLine.LineWidth = 2;

            var yAxisLine = Canvas.Plot.Add.Line(0, -10000, 0, 10000);
            yAxisLine.Color = ScottPlot.Color.FromHex("#00000077");
            yAxisLine.LineWidth = 2;

            Canvas.Plot.Axes.Rules.Clear();
            var squareRule = new ScottPlot.AxisRules.SquarePreserveX(Canvas.Plot.Axes.Bottom, Canvas.Plot.Axes.Left);
            Canvas.Plot.Axes.Rules.Add(squareRule);
            Canvas.Plot.Axes.Rules.Add(new ScottPlot.AxisRules.MaximumBoundary(
                Canvas.Plot.Axes.Bottom,
                Canvas.Plot.Axes.Left,
                new ScottPlot.AxisLimits(-100, 100, -100, 100)));

            Canvas.UserInputProcessor.RightClickDragZoom(false);

            var tickerX = new ScottPlot.TickGenerators.NumericAutomatic { TargetTickCount = 20 };
            Canvas.Plot.Axes.Bottom.TickGenerator = tickerX;
            var tickerY = new ScottPlot.TickGenerators.NumericAutomatic { TargetTickCount = 20 };
            Canvas.Plot.Axes.Left.TickGenerator = tickerY;

            Canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            Canvas.MouseMove += Canvas_MouseMove;
            Canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;

            RenderPoints();
            Move();
        }

        [RelayCommand(CanExecute = nameof(CanAddPoint))]
        public void Add()
        {
            IsAddMode = true;
            IsMoveMode = IsDragMode = IsDeleteMode = false;
            Cursor = "Cross";
            Canvas.UserInputProcessor.Disable();
        }
        private bool CanAddPoint() => !IsAddMode && ControlPoints.Count < 31;

        [RelayCommand(CanExecute = nameof(CanMoveCanvas))]
        public void Move()
        {
            IsMoveMode = true;
            IsAddMode = IsDragMode = IsDeleteMode = false;
            Cursor = "SizeAll";
            Canvas.UserInputProcessor.Enable();
        }
        private bool CanMoveCanvas() => !IsMoveMode;

        [RelayCommand(CanExecute = nameof(CanDragPoint))]
        public void Drag()
        {
            IsDragMode = true;
            IsAddMode = IsMoveMode = IsDeleteMode = false;
            Cursor = "Hand";
            Canvas.UserInputProcessor.Disable();
        }
        private bool CanDragPoint() => !IsDragMode;

        [RelayCommand(CanExecute = nameof(CanDeletePoint))]
        public void Delete()
        {
            IsDeleteMode = true;
            IsAddMode = IsMoveMode = IsDragMode = false;
            Cursor = "No";
            Canvas.UserInputProcessor.Disable();
        }
        private bool CanDeletePoint() => !IsDeleteMode;
        [RelayCommand]
        public async Task DeleteAll()
        {
            if (ControlPoints.Count == 0) return;

            var result = await _popUpService.CallQuestionContentDialog(
                "Clear Canvas?",
                "Are you sure you want to delete all points and curves? This action cannot be undone."
            );

            if (result != Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                return;
            }

            ControlPoints.Clear();
            _cachedMatrix = null;
            ParametricTable = null;
            MatrixTable = null;
            MatrixRowElements.Clear();
            _draggedPoint = null;

            RenderPoints();

            AddCommand.NotifyCanExecuteChanged();
            AddCoordinatesCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanAddCoordinatesPoint))]
        public void AddCoordinates()
        {
            if (ControlPoints.Count >= 31)
            {
                _popUpService.CallErrorMessageBox("Control point limit hit", "Maximum control point count reached... To add control points consider deleting some beforehand!");
                return;
            }

            if (X_tb > 100 || X_tb < -100 || Y_tb > 100 || Y_tb < -100)
            {
                _popUpService.CallErrorMessageBox("Invalid control point coordinates!","Invalid coordinates for control points were entered... The acceptable range is [-100;100] for X and Y.");
                return;
            }

            string newName = $"P{ControlPoints.Count}";
            ControlPoints.Add(new ControlPoint(newName, X_tb, Y_tb, isAnchor: true));

            if (ControlPoints.Count > 2)
            {
                ControlPoints[ControlPoints.Count - 2].IsAnchor = false;
            }

            RenderPoints();
            AddCommand.NotifyCanExecuteChanged();
            AddCoordinatesCommand.NotifyCanExecuteChanged();
        }

        private bool CanAddCoordinatesPoint()
        {
            return ControlPoints.Count <= 31;
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMoveMode) return;

            System.Windows.Point mousePos = e.GetPosition(Canvas);
            Pixel mousePixel = new Pixel((float)(mousePos.X * Canvas.DisplayScale), (float)(mousePos.Y * Canvas.DisplayScale));
            Coordinates coordinates = Canvas.Plot.GetCoordinates(mousePixel);

            if (IsAddMode)
            {
                if (ControlPoints.Count >= 31)
                {
                    _popUpService.CallErrorMessageBox("Control point limit hit", "Maxium control point count reached... To add control points consider deleting some beforehand!");
                    return;
                }

                double mathX = Math.Round(coordinates.X, 2);
                double mathY = Math.Round(coordinates.Y, 2);

                int insertIndex = ControlPoints.Count;
                double minDistance = 15; 

                for (int i = 0; i < ControlPoints.Count - 1; i++)
                {
                    Pixel pA = Canvas.Plot.GetPixel(new Coordinates(ControlPoints[i].X, ControlPoints[i].Y));
                    Pixel pB = Canvas.Plot.GetPixel(new Coordinates(ControlPoints[i + 1].X, ControlPoints[i + 1].Y));

                    double dist = GetDistanceToSegment(mousePixel, pA, pB);

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        insertIndex = i + 1;
                    }
                }

                ControlPoints.Insert(insertIndex, new ControlPoint("", mathX, mathY, isAnchor: false));
                RefreshPointData();

                RenderPoints();
                AddCommand.NotifyCanExecuteChanged();
            }
            else if (IsDragMode)
            {
                double minDistance = 15;
                ControlPoint closestPoint = null;

                foreach (var cp in ControlPoints)
                {
                    Pixel cpPixel = Canvas.Plot.GetPixel(new Coordinates(cp.X, cp.Y));
                    double dx = cpPixel.X - mousePixel.X;
                    double dy = cpPixel.Y - mousePixel.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestPoint = cp;
                    }
                }

                _draggedPoint = closestPoint;
            }
            else if (IsDeleteMode)
            {
                double minDistance = 15;
                ControlPoint pointToDelete = null;

                foreach (var cp in ControlPoints)
                {
                    Pixel cpPixel = Canvas.Plot.GetPixel(new Coordinates(cp.X, cp.Y));
                    double dx = cpPixel.X - mousePixel.X;
                    double dy = cpPixel.Y - mousePixel.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        pointToDelete = cp;
                    }
                }
                if (pointToDelete != null)
                {
                    ControlPoints.Remove(pointToDelete);
                    RefreshPointData();
                    RenderPoints();
                    AddCommand.NotifyCanExecuteChanged();
                }
            }
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragMode || _draggedPoint == null) return;

            System.Windows.Point mousePos = e.GetPosition(Canvas);
            Pixel mousePixel = new Pixel((float)(mousePos.X * Canvas.DisplayScale), (float)(mousePos.Y * Canvas.DisplayScale));
            Coordinates coordinates = Canvas.Plot.GetCoordinates(mousePixel);

            double newX = Math.Clamp(Math.Round(coordinates.X, 2), -100, 100);
            double newY = Math.Clamp(Math.Round(coordinates.Y, 2), -100, 100);

            _draggedPoint.X = newX;
            _draggedPoint.Y = newY;

            RenderPoints();
        }

        private void RenderPoints()
        {
            bool shouldDrawParametric = _parametricCurve != null;
            bool shouldDrawMatrix = _matrixCurve != null;

            foreach (var marker in _plottedMarkers)
            {
                Canvas.Plot.Remove(marker);
            }
            _plottedMarkers.Clear();

            foreach (var label in _plottedLabels)
            {
                Canvas.Plot.Remove(label);
            }
            _plottedLabels.Clear();

            if (_plottedLine != null)
            {
                Canvas.Plot.Remove(_plottedLine);
                _plottedLine = null;
            }

            if (_parametricCurve != null)
            {
                Canvas.Plot.Remove(_parametricCurve);
                _parametricCurve = null;
            }
            if (_matrixCurve != null)
            {
                Canvas.Plot.Remove(_matrixCurve);
                _matrixCurve = null;
            }

            if (ControlPoints.Count == 0)
            {
                Canvas.Refresh();
                return;
            }

            double[] xs = new double[ControlPoints.Count];
            double[] ys = new double[ControlPoints.Count];

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                xs[i] = ControlPoints[i].X;
                ys[i] = ControlPoints[i].Y;
            }

            if (ControlPoints.Count > 1)
            {
                _plottedLine = Canvas.Plot.Add.Scatter(xs, ys);
                _plottedLine.MarkerSize = 0;
                _plottedLine.LineWidth = 2;
                _plottedLine.Color = ScottPlot.Color.FromHex(Line_CB.Code);
            }

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                var marker = Canvas.Plot.Add.Marker(ControlPoints[i].X, ControlPoints[i].Y);
                marker.MarkerStyle.FillColor = ScottPlot.Color.FromHex(Point_CB.Code);
                marker.Size = 10;
                _plottedMarkers.Add(marker);

                var textPlot = Canvas.Plot.Add.Text(ControlPoints[i].Name, ControlPoints[i].X, ControlPoints[i].Y);
                textPlot.LabelFontColor = ScottPlot.Colors.Black;
                textPlot.LabelFontSize = 14;
                textPlot.LabelAlignment = ScottPlot.Alignment.LowerCenter;
                textPlot.OffsetY = -12;
                textPlot.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.8);

                _plottedLabels.Add(textPlot);
            }

            if (ControlPoints.Count >= 2)
            {
                if (shouldDrawParametric && IntervalsParametric > 0)
                {
                    var paramData = _bezierService.CalculateParametric(ControlPoints.ToList(), 1.0 / IntervalsParametric).Points;
                    BuildCurve(paramData, ref _parametricCurve, Bezier_Parametric_CB.Code);
                    _parametricCurve.IsVisible = IsParametricVisible;
                }

                if (shouldDrawMatrix && IntervalsMatrix > 0)
                {
                    var matrixData = _bezierService.CalculateMatrix(ControlPoints.ToList(), 1.0 / IntervalsMatrix).Points;
                    BuildCurve(matrixData, ref _matrixCurve, Bezier_Matrix_CB.Code);
                    _matrixCurve.IsVisible = IsMatrixVisible;
                }
            }

            Canvas.Refresh();
        }
        private double GetDistanceToSegment(Pixel p, Pixel a, Pixel b)
        {
            double l2 = Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2);
            if (l2 == 0) return Math.Sqrt(Math.Pow(p.X - a.X, 2) + Math.Pow(p.Y - a.Y, 2));

            double t = ((p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y)) / l2;
            t = Math.Max(0, Math.Min(1, t));

            double projectionX = a.X + t * (b.X - a.X);
            double projectionY = a.Y + t * (b.Y - a.Y);

            return Math.Sqrt(Math.Pow(p.X - projectionX, 2) + Math.Pow(p.Y - projectionY, 2));
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDragMode && _draggedPoint != null)
            {
                _draggedPoint = null;

                if (ControlPoints.Count >= 2)
                {
                    if (_parametricCurve != null && IntervalsParametric > 0)
                    {
                        var pData = _bezierService.CalculateParametric(ControlPoints.ToList(), 1.0 / IntervalsParametric).Points;
                        CalculateParametricTable(new List<ParametricBezierPoint>(pData));
                    }

                    if (_matrixCurve != null && IntervalsMatrix > 0)
                    {
                        var mData = _bezierService.CalculateMatrix(ControlPoints.ToList(), 1.0 / IntervalsMatrix).Points;
                        DataTable dt;
                        if ((dt = CalculateTable(new List<BezierPoint>(mData))) != null)
                            MatrixTable = dt;
                    }
                }
            }
        }

        private void ControlPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ControlPoint newPoint in e.NewItems)
                {
                    newPoint.PropertyChanged += ControlPoint_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ControlPoint oldPoint in e.OldItems)
                {
                    oldPoint.PropertyChanged -= ControlPoint_PropertyChanged;
                }
            }
        }

        private void ControlPoint_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ControlPoint.X) || e.PropertyName == nameof(ControlPoint.Y))
            {
                RenderPoints();
            }
        }

        [RelayCommand]
        public void DeleteSpecificPoint(ControlPoint pointToDelete)
        {
            if (pointToDelete != null && ControlPoints.Contains(pointToDelete))
            {
                ControlPoints.Remove(pointToDelete);
                RefreshPointData();
                RenderPoints();
                AddCommand.NotifyCanExecuteChanged();
                AddCoordinatesCommand.NotifyCanExecuteChanged();
            }
        }

        private void RefreshPointData()
        {
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (i == 0 || i == ControlPoints.Count - 1)
                    ControlPoints[i].IsAnchor = true;
                else
                    ControlPoints[i].IsAnchor = false;

                ControlPoints[i].Name = $"P{i}";
            }
        }

        // Parametric
        [RelayCommand]
        public async Task CalculateParametric()
        {
            if (ControlPoints.Count < 2 || IntervalsParametric <= 0)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "There's not enough of control points and/or intervals, try again!");
                return;
            }
            if (IntervalsParametric > 100)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "Too many intervals, max is 100, try again!");
                return;
            }
           ParametricBezierPoint[] tableData = _bezierService.CalculateParametric(ControlPoints.ToList(), 1.0 / IntervalsParametric).Points;
           CalculateParametricTable(new List<ParametricBezierPoint>(tableData));
        }
        private void CalculateParametricTable(List<ParametricBezierPoint> tableData)
        {
            if (tableData == null || tableData.Count == 0) return;

            DataTable dt = new DataTable();

            dt.Columns.Add("№", typeof(int));
            dt.Columns.Add("t", typeof(double));
            dt.Columns.Add("X", typeof(double));
            dt.Columns.Add("Y", typeof(double));

            int n = tableData[0].BernsteinValues.Count - 1;

            for (int i = 0; i <= n; i++)
            {
                dt.Columns.Add($"B({n},{i})", typeof(double));
            }

            foreach (var item in tableData)
            {
                DataRow row = dt.NewRow();

                row["№"] = item.Id;
                row["t"] = Math.Round(item.T, 5); 
                row["X"] = Math.Round(item.X, 2);
                row["Y"] = Math.Round(item.Y, 2);

                for (int i = 0; i <= n; i++)
                {
                    row[$"B({n},{i})"] = Math.Round(item.BernsteinValues[i], 4);
                }

                dt.Rows.Add(row);
            }
            ParametricTable = dt;
        }
        [RelayCommand]
        public async Task BuildParametric()
        {
            if (ControlPoints.Count < 2 || IntervalsParametric <= 0)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "There's not enough of control points and/or intervals, try again!");
                return;
            }
            if (IntervalsParametric > 100)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "Too many intervals, max is 100, try again!");
                return;
            }
            IsParametricVisible = true;
            ParametricBezierPoint[] curveData = _bezierService.CalculateParametric(ControlPoints.ToList(), 1.0 / IntervalsParametric).Points;
            BuildCurve(curveData, ref _parametricCurve, Bezier_Parametric_CB.Code);
        }
        [RelayCommand]
        public async Task CalculateMatrix()
        {
            if (ControlPoints.Count < 2 || IntervalsMatrix <= 0)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "There's not enough of control points and/or intervals, try again!");
                return;
            }
            if (IntervalsMatrix > 100)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "Too many intervals, max is 100, try again!");
                return;
            }
            var response = (MatrixBezierResponse)_bezierService.CalculateMatrix(ControlPoints.ToList(), 1.0 / IntervalsMatrix);
            _cachedMatrix = response.N;
            DataTable dt;
            if ((dt = CalculateTable(new List<BezierPoint>(response.Points))) != null)
                MatrixTable = dt;
        }
        private DataTable CalculateTable(List<BezierPoint> tableData)
        {
            if (tableData == null || tableData.Count == 0) return null;

            DataTable dt = new DataTable();

            dt.Columns.Add("№", typeof(int));
            dt.Columns.Add("t", typeof(double));
            dt.Columns.Add("X", typeof(double));
            dt.Columns.Add("Y", typeof(double));


            foreach (var item in tableData)
            {
                DataRow row = dt.NewRow();

                row["№"] = item.Id;
                row["t"] = Math.Round(item.T, 5);
                row["X"] = Math.Round(item.X, 2);
                row["Y"] = Math.Round(item.Y, 2);
                dt.Rows.Add(row);
            }
            return dt;
        }
        [RelayCommand]
        public async Task BuildMatrix()
        {
            if (ControlPoints.Count < 2 || IntervalsMatrix <= 0)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "There's not enough control points and/or intervals, try again!");
                return;
            }
            if (IntervalsMatrix > 100)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "Too many intervals, max is 100, try again!");
                return;
            }
            IsMatrixVisible = true;
            BezierPoint[] curveData = _bezierService.CalculateMatrix(ControlPoints.ToList(), 1.0 / IntervalsMatrix).Points;
            BuildCurve(curveData, ref _matrixCurve, Bezier_Matrix_CB.Code);
        }
        
        public void BuildCurve(BezierPoint[] curveData, ref ScottPlot.Plottables.Scatter curvePlot, string hexColor)
        {
            double[] xs = new double[curveData.Length];
            double[] ys = new double[curveData.Length];

            for (int i = 0; i < curveData.Length; i++)
            {
                xs[i] = curveData[i].X;
                ys[i] = curveData[i].Y;
            }

            if (curvePlot != null)
            {
                Canvas.Plot.Remove(curvePlot);
            }

            curvePlot = Canvas.Plot.Add.Scatter(xs, ys);

            curvePlot.MarkerSize = 0;
            curvePlot.LineWidth = 2;
            curvePlot.Color = ScottPlot.Color.FromHex(hexColor);

            Canvas.Refresh();
        }
        [RelayCommand]
        public async Task SearchRow()
        {
            if (_cachedMatrix == null || _cachedMatrix.Length == 0)
            {
                await _popUpService.CallErrorMessageBox("Matrix Not Found", "Please calculate the matrix curve first before searching for a row.");
                return;
            }

            int targetRow = MatrixRow - 1;

            if (targetRow < 0 || targetRow >= _cachedMatrix.Length)
            {
                await _popUpService.CallErrorMessageBox("Invalid Row", $"The matrix has {_cachedMatrix.Length} rows. Please enter a value between 1 and {_cachedMatrix.Length}.");
                return;
            }

            double[] rowData = _cachedMatrix[targetRow];
            var newElements = new List<MatrixRowElement>();

            for (int col = 0; col < rowData.Length; col++)
            {
                newElements.Add(new MatrixRowElement(MatrixRow, col + 1, rowData[col]));
            }

            MatrixRowElements.Clear();
            foreach (var element in newElements)
            {
                MatrixRowElements.Add(element);
            }
        }

        [ObservableProperty]
        private bool _isMatrixVisible = true;
        [ObservableProperty]
        private string _matrixVisibilityBtnToolTip = "Hide";

        [RelayCommand]
        public void ToggleMatrix()
        {
            IsMatrixVisible = !IsMatrixVisible;
            if (IsMatrixVisible) MatrixVisibilityBtnToolTip = "Hide";
            else MatrixVisibilityBtnToolTip = "Show";
            if (_matrixCurve != null)
            {
                _matrixCurve.IsVisible = IsMatrixVisible;
                Canvas.Refresh();
            }
        }

        [ObservableProperty]
        private bool _isParametricVisible = true;
        [ObservableProperty]
        private string _parametricVisibilityBtnToolTip = "Hide";

        [RelayCommand]
        public void ToggleParametric()
        {
            IsParametricVisible = !IsParametricVisible;
            if (IsParametricVisible) ParametricVisibilityBtnToolTip = "Hide";
            else ParametricVisibilityBtnToolTip = "Show";
            if (_parametricCurve != null)
            {
                _parametricCurve.IsVisible = IsParametricVisible;
                Canvas.Refresh();
            }
        }

        [RelayCommand]
        public async Task Tabulate()
        {
            if (ControlPoints.Count < 2 || MinT_TB > 1 || MinT_TB < 0 || MaxT_TB > 1 || MaxT_TB < 0 || MinT_TB > MaxT_TB || StepT_TB <= 0)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "There's not enough of control points, try again!");
                return;
            }
            if (MinT_TB > 1 || MinT_TB < 0 || MaxT_TB > 1 || MaxT_TB < 0 || MinT_TB > MaxT_TB || StepT_TB <= 0)
            {
                await _popUpService.CallErrorMessageBox("Error occured", "Invalid t range and/or t step, try again!");
                return;
            }
            BezierPoint[] points = new BezierPoint[0];
            if(Method_CB == "Parametric")
            {
                var response = _bezierService.CalculateParametric(ControlPoints.ToList(), StepT_TB, MinT_TB, MaxT_TB);
                points = response.Points;
            }
            else
            {
                var response = _bezierService.CalculateMatrix(ControlPoints.ToList(), StepT_TB, MinT_TB, MaxT_TB);
                points = response.Points;
            }
            DataTable dt;
            if ((dt = CalculateTable(new List<BezierPoint>(points))) != null)
                TabulationTable = dt;
        }
    }
}
