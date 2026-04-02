using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_03_CG.MVVM.Model
{
    public class AddGeometricFractalDTO : AddFractalDTO
    {
        public string Initializer { get; set; }
        public string Generator { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double SideLength { get; set; }
        public int RotationAngle { get; set; }
        public string LineColor { get; set; }
    }
}
