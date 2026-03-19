using System;
using System.Collections.Generic;
using System.Text;
using Lab_02_CG.Domain;
using Lab_02_CG.Services.Models;

namespace Lab_02_CG.Services.Abstractions
{
    public interface IBezierService
    {
        public ParametricBezierResponse CalculateParametric(List<ControlPoint> controlPoints, double dT, double MinT = 0, double MaxT = 1);
        public MatrixBezierResponse CalculateMatrix(List<ControlPoint> controlPoints, double dT, double MinT = 0, double MaxT = 1);
    }
}
