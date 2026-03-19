using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_01_CG.Data.Entities
{
    // Сутність квадрат
    public class Square
    {
        public int Id { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double SideLength { get; set; }
        public double RotationAngle { get; set; } // degrees
        public string OutlineColor { get; set; } // #HEX
        public string FillColor { get; set; } // #HEX
        public int InnerCircleId { get; set; }
        public Circle InnerCircle { get; set; }
        public int OuterCircleId { get; set; }
        public Circle OuterCircle { get; set; }
    }
}
