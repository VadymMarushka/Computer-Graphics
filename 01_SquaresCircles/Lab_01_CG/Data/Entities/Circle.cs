using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_01_CG.Data.Entities
{
    // Сутність Коло
    public class Circle
    {
        public int Id { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Radius { get; set; }
        public string OutlineColor { get; set; } // #HEX
        public string FillColor { get; set; } // #HEX
    }
}
