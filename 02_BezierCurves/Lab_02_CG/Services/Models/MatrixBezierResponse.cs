using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_02_CG.Services.Models
{
    public partial class MatrixBezierResponse : BezierResponse<BezierPoint>
    {
        [ObservableProperty]
        private double[][] _n;

        public MatrixBezierResponse(double[][] n , BezierPoint[] bezierPoints) : base(bezierPoints)
        {
            _n = n;
        }
        public MatrixBezierResponse()
        {
            _n = new double[0][];
        }
    }
}
