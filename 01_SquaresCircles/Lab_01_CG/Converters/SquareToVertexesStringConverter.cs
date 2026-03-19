using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Lab_01_CG.Converters
{
    public class SquareToVertexesStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double x && values[1] is double y && values[2] is double a && values[3] is double angle)
            {
                double d = a / 2;
                Coordinates[] v =
{
                new Coordinates (x-d, y-d),
                new Coordinates (x-d,y+d),
                new Coordinates (x+d,y+d),
                new Coordinates (x+d,y-d),
            };

                double alpha = angle * Math.PI / 180;
                double sinA = Math.Sin(alpha);
                double cosA = Math.Cos(alpha);

                Coordinates[] tp =
                {
                new Coordinates ( (x + (v[0].X - x) * cosA - (v[0].Y - y) * sinA) , (y + (v[0].X - x) * sinA + (v[0].Y - y) * cosA) ),
                new Coordinates ( (x + (v[1].X - x) * cosA - (v[1].Y - y) * sinA) , (y + (v[1].X - x) * sinA + (v[1].Y - y) * cosA) ),
                new Coordinates ( (x + (v[2].X - x) * cosA - (v[2].Y - y) * sinA) , (y + (v[2].X - x) * sinA + (v[2].Y - y) * cosA) ),
                new Coordinates ( (x + (v[3].X - x) * cosA - (v[3].Y - y) * sinA) , (y + (v[3].X - x) * sinA + (v[3].Y - y) * cosA) ),
            };
                return $"({Math.Round(tp[0].X , 2)} ; {Math.Round(tp[0].Y, 2)})," +
                    $" ({Math.Round(tp[1].X, 2)} ; {Math.Round(tp[1].Y, 2)})," +
                    $" ({Math.Round(tp[2].X, 2)} ; {Math.Round(tp[2].Y, 2)})," +
                    $" ({Math.Round(tp[3].X, 2)} ; {Math.Round(tp[3].Y, 2)})";
            }
            return " ";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
