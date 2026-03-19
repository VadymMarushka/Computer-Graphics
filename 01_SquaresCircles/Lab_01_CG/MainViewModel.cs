using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab_01_CG.Data.Entities;
using Lab_01_CG.Data.Services;
using Lab_01_CG.Model;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using ScottPlot;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lab_01_CG
{
    public partial class MainViewModel : ObservableObject
    {
        public WpfPlot Canvas { get; set; }

        [ObservableProperty]
        private List<Square> _squares;

        [ObservableProperty]
        private string _centerX_TB; // суфікс _TB означає, що властивість буде прив'язана до компоненти TextBox
        [ObservableProperty]
        private string _centerY_TB;
        [ObservableProperty]
        private string _sideLength_TB;
        [ObservableProperty]
        private string _angle_TB;

        [ObservableProperty]
        private HexColor _squareLine_CB; // суфікс _CB означає, що властивість буде прив'язана до компоненти ComboBox
        [ObservableProperty]
        private HexColor _squareFill_CB;

        [ObservableProperty]
        private HexColor _innerLine_CB;
        [ObservableProperty]
        private HexColor _innerFill_CB;

        [ObservableProperty]
        private HexColor _outerLine_CB;
        [ObservableProperty]
        private HexColor _outerFill_CB;

        [ObservableProperty]
        private List<string> _colorNames;

        [ObservableProperty]
        private List<string> _colorNamesForSquareLine;

        private readonly SquareAndCirclesDataService _dataService;
        public MainViewModel(SquareAndCirclesDataService dataService)
        {
            _dataService = dataService;
            CenterX_TB = CenterY_TB = SideLength_TB = Angle_TB = "";
            SquareLine_CB = new HexColor();
            SquareFill_CB = new HexColor();
            InnerLine_CB = new HexColor();
            InnerFill_CB = new HexColor();
            OuterLine_CB = new HexColor();
            OuterFill_CB = new HexColor();
            ColorNames = HexColor.Colors.Keys.ToList();
            ColorNamesForSquareLine = HexColor.Colors.Keys.ToList();
            ColorNamesForSquareLine.Remove("Transparent");
            Canvas = new WpfPlot();
            GetSquaresAsync();
            DrawShapes();
        }
        public async Task GetSquaresAsync()
        {
            Squares = await _dataService.GetAllSquaresAsync();
        }

        [RelayCommand]
        public async Task DeleteListItem(int id)
        {
            DeleteShapeFromCanvas(id);
            DrawShapes();
        }
        public async Task DeleteShapeFromCanvas(int id)
        {
            await _dataService.DeleteSquare(id);
            Squares = await _dataService.GetAllSquaresAsync();
        }
        [RelayCommand] 
        public async Task AddShape()
        {
            // Перевірка на коректність вхідних даних...
            if (!double.TryParse(CenterX_TB.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double x))
            {
                MessageBox.Show("Incorrect center's X was entered! Coordinate must be a real number.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!double.TryParse(CenterY_TB.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
            {
                MessageBox.Show("Incorrect center's Y was entered! Coordinate must be a real number.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!double.TryParse(SideLength_TB.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double a) 
                || a < 0)
            {
                MessageBox.Show("Incorrect side length was entered! Side length must be a positive non-zero real number.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!double.TryParse(Angle_TB.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double angle))
            {
                MessageBox.Show("Incorrect angle was entered! Angle must be a real number.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // створюємо новий квадрат та його описане та вписане кола
            Square square = new Square
            {
                CenterX = Math.Round(x,3), // заокруглюємо до трьох цифр після коми
                CenterY = Math.Round(y,3),
                SideLength = Math.Round(a, 3),
                RotationAngle = Math.Round(angle),
                OutlineColor = SquareLine_CB.Code,
                FillColor = SquareFill_CB.Code,
                InnerCircle = new Circle
                {
                    CenterX = Math.Round(x,3),
                    CenterY = Math.Round(y,3),
                    Radius = Math.Round(a/2,3),
                    OutlineColor = InnerLine_CB.Code,
                    FillColor = InnerFill_CB.Code,
                },
                OuterCircle = new Circle
                {
                    CenterX = Math.Round(x, 3),
                    CenterY = Math.Round(y, 3),
                    Radius = Math.Round(a/Math.Sqrt(2), 3),
                    OutlineColor = OuterLine_CB.Code,
                    FillColor = OuterFill_CB.Code,
                }
            };

            double tmp =
    Math.Max(square.CenterX + square.OuterCircle.Radius,
    Math.Max(square.CenterY + square.OuterCircle.Radius,
    Math.Abs(Math.Min(square.CenterX - square.OuterCircle.Radius,
    square.CenterY - square.OuterCircle.Radius))));

            if (tmp > 10000)
            {
                MessageBox.Show("Entered shape spreads beyond the exepected limits, try entering lower values for the center coordinates or/and side length...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Записуємо зміни до БД та перемалбовуємо фігури
            await _dataService.AddSquare(square);
            await GetSquaresAsync();
            await DrawShapes();
        }
        public void SetCanvasLimits(double minX, double maxX, double minY, double maxY)
        {
            // створення осей абсцис та ординат по центру координатної площини

            Canvas.Plot.Axes.Bottom.FrameLineStyle.Color = ScottPlot.Colors.Blue;
            ScottPlot.Plottables.FloatingAxis floatingX = new(Canvas.Plot.Axes.Bottom);
            ScottPlot.Plottables.FloatingAxis floatingY = new(Canvas.Plot.Axes.Left);

            Canvas.Plot.Axes.Frameless();
            Canvas.Plot.Add.Plottable(floatingX);
            Canvas.Plot.Add.Plottable(floatingY);
            Canvas.IsHitTestVisible = false;

            Canvas.Plot.DataBackground.Color = ScottPlot.Colors.White;
            Canvas.Plot.Grid.LinePattern = LinePattern.Dotted;
            Canvas.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#00000044");

            // Встановлення границь координатної площини
            Canvas.Plot.Axes.SetLimits(minX + (minX * 0.05), maxX + (maxX * 0.05), minY + (minY * 0.05), maxY + (maxY * 0.05));
            Canvas.Refresh();
        }
        private double range = 1;
        public void AddShapeToCanvas(Square square)
        {
            // Створення та додавання нового квадрату на координатну площину
            Circle innerCircle = square.InnerCircle;
            Circle outerCircle = square.OuterCircle;

            Coordinates location = new(square.CenterX-(square.SideLength/2), square.CenterY - (square.SideLength/2));
            CoordinateSize size = new(square.SideLength, square.SideLength);
            CoordinateRect rect = new(location, size);

            var outerCircle_Canvas = Canvas.Plot.Add.Circle(outerCircle.CenterX, outerCircle.CenterY, outerCircle.Radius);
            outerCircle_Canvas.LineStyle.Color = ScottPlot.Color.FromHex(outerCircle.OutlineColor + ((outerCircle.OutlineColor.Length < 8) ? "FF" : ""));// тут RGBA, а не ARGB виявляється :\
            outerCircle_Canvas.FillStyle.Color = ScottPlot.Color.FromHex(outerCircle.FillColor + ((outerCircle.FillColor.Length < 8) ? "66" : ""));
            outerCircle_Canvas.LineStyle.Width = 2.5f;

            double x = square.CenterX;
            double y = square.CenterY;
            double d = square.SideLength/2; // половина сторони

            Coordinates[] v = // вершини квадрата до обертання
{
                new Coordinates (x-d, y-d),
                new Coordinates (x-d,y+d),
                new Coordinates (x+d,y+d),
                new Coordinates (x+d,y-d),
            };

            // x' = Ox + (x - Ox) * cos(a) - (y - Oy) * sin(a)
            // y' = Oy + (x - Ox) * sin(a) + (y - Oy) * cos(a)

            double a = square.RotationAngle * Math.PI / 180;
            double sinA = Math.Sin(a);
            double cosA = Math.Cos(a);

            Coordinates[] TransformedPoints =
            {
                new Coordinates ( (x + (v[0].X - x) * cosA - (v[0].Y - y) * sinA) , (y + (v[0].X - x) * sinA + (v[0].Y - y) * cosA) ),
                new Coordinates ( (x + (v[1].X - x) * cosA - (v[1].Y - y) * sinA) , (y + (v[1].X - x) * sinA + (v[1].Y - y) * cosA) ),
                new Coordinates ( (x + (v[2].X - x) * cosA - (v[2].Y - y) * sinA) , (y + (v[2].X - x) * sinA + (v[2].Y - y) * cosA) ),
                new Coordinates ( (x + (v[3].X - x) * cosA - (v[3].Y - y) * sinA) , (y + (v[3].X - x) * sinA + (v[3].Y - y) * cosA) ),
            };

            var p = Canvas.Plot.Add.Polygon(TransformedPoints);
            p.LineStyle.Color = ScottPlot.Color.FromHex(square.OutlineColor + ((square.OutlineColor.Length < 8) ? "FF" : ""));
            p.FillStyle.Color = ScottPlot.Color.FromHex(square.FillColor + ((square.FillColor.Length < 8) ? "66" : ""));
            p.LineStyle.Width = 2.5f;
            /*
            Coordinates[] tr1_p =
{
                new Coordinates(TransformedPoints[0].X , TransformedPoints[0].Y),
                new Coordinates(TransformedPoints[1].X, TransformedPoints[1].Y),
                new Coordinates(x , y),
            };
            
            Coordinates[] tr2_p =
                {
                new Coordinates(TransformedPoints[1].X , TransformedPoints[1].Y),
                new Coordinates(TransformedPoints[2].X, TransformedPoints[2].Y),
                new Coordinates(x , y),
            };
            
            
            Coordinates[] tr3_p =
            {
                new Coordinates(TransformedPoints[2].X , TransformedPoints[2].Y),
                new Coordinates(TransformedPoints[3].X, TransformedPoints[3].Y),
                new Coordinates(x , y),
            };
           
            Coordinates[] tr4_p =
            {
                new Coordinates(TransformedPoints[3].X , TransformedPoints[3].Y),
                new Coordinates(TransformedPoints[0].X, TransformedPoints[0].Y),
                new Coordinates(x , y),
            };
            
            
            var tr1 = Canvas.Plot.Add.Polygon(tr1_p);
            var tr2 = Canvas.Plot.Add.Polygon(tr2_p);
            var tr3 = Canvas.Plot.Add.Polygon(tr3_p);
            var tr4 = Canvas.Plot.Add.Polygon(tr4_p);

            tr1.FillStyle.Color = tr2.FillStyle.Color = tr3.FillStyle.Color = tr4.FillStyle.Color = ScottPlot.Color.FromHex("#FFFF00FF");
            
            var tr1 = Canvas.Plot.Add.Polygon(tr1_p);
            var tr2 = Canvas.Plot.Add.Polygon(tr2_p);
            var tr3 = Canvas.Plot.Add.Polygon(tr3_p);
            var tr4 = Canvas.Plot.Add.Polygon(tr4_p);

            */

            //var tr2 = Canvas.Plot.Add.Polygon(tr2_p);
            //var tr3 = Canvas.Plot.Add.Polygon(tr3_p);
            //var tr4 = Canvas.Plot.Add.Polygon(tr4_p);

            var innerCircle_Canvas = Canvas.Plot.Add.Circle(innerCircle.CenterX, square.CenterY, innerCircle.Radius);
            innerCircle_Canvas.LineStyle.Color = ScottPlot.Color.FromHex(innerCircle.OutlineColor + ((innerCircle.OutlineColor.Length < 8) ? "FF" : ""));
            innerCircle_Canvas.FillStyle.Color = ScottPlot.Color.FromHex(innerCircle.FillColor + ((innerCircle.FillColor.Length < 8) ? "66" : ""));
            innerCircle_Canvas.LineStyle.Width = 2.5f;

            var o = Canvas.Plot.Add.Marker(new Coordinates(x, y));
            var v1 = Canvas.Plot.Add.Marker(TransformedPoints[0]);
            var v2 = Canvas.Plot.Add.Marker(TransformedPoints[1]);
            var v3 = Canvas.Plot.Add.Marker(TransformedPoints[2]);
            var v4 = Canvas.Plot.Add.Marker(TransformedPoints[3]);

            o.Color = v1.Color = v2.Color = v3.Color = v4.Color = ScottPlot.Color.FromHex("#111111");
            o.Size = v1.Size = v2.Size = v3.Size = v4.Size = 7f;

            Canvas.Refresh();
        }
        public async Task DrawShapes()
        {
            Canvas.Plot.Clear();

            double maxPoint = 0;
            bool illegalShapes = false;
            foreach (Square square in Squares)
            {
                // обчислення максимального зміщення фігури
                double tmp =
                    Math.Max( square.CenterX + square.OuterCircle.Radius,
                    Math.Max(square.CenterY + square.OuterCircle.Radius,
                    Math.Abs(Math.Min(square.CenterX - square.OuterCircle.Radius,
                    square.CenterY - square.OuterCircle.Radius))));
                if (tmp > 10000)
                {
                    illegalShapes = true;
                    await DeleteShapeFromCanvas(square.Id);
                }
                else
                {
                    if (tmp > maxPoint) maxPoint = tmp;
                    AddShapeToCanvas(square);
                }
            }
            if(illegalShapes)
            {
                MessageBox.Show("Some shapes went out of expected bounds, therefore they've been removed...","Some shapes removed",MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //Підбір меж координатної площини

            // 1 2 5 10 20 50 100 200 500 1000 2000 5000 10000
            // q = (2 , 2.5 , 2), (2, 2.5 , 2) ...
            double[] q = new double[]{ 2, 2.5, 2 };
            double range = 1;
            for(int i = 0; i < 12;i++)
            {
                if (range > maxPoint) break;
                range *= q[i % 3];
            }
            SetCanvasLimits(-range, range, -range, range);
            Canvas.Refresh();
        }
    }
}
