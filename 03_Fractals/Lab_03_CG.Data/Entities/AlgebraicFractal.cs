using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_03_CG.Data.Entities
{
    public class AlgebraicFractal : Fractal
    {
        public string Formula { get; set; }
        public string Family { get; set; }
        public string Palette { get; set; }
        public double Z0_Real {  get; set; }
        public double Z0_Imaginary { get; set; }
        public double C_Real {  get; set; }
        public double C_Imaginary { get; set; }
        public double EscapeRadius { get; set; }
        public int MaxIterations { get; set; }
    }
}
