using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_02_CG.Services.Models
{
    public partial class ParametricBezierResponse : BezierResponse<ParametricBezierPoint>
    {
        public ParametricBezierResponse() { }
        public ParametricBezierResponse(ParametricBezierPoint[] points) : base(points) { }
    }
    public partial class ParametricBezierPoint : BezierPoint
    {
        [ObservableProperty]
        private List<double> _bernsteinValues;

        public ParametricBezierPoint(int id, double t,  double x, double y, List<double> bernsteinValues)
            : base(id, t, x , y)
        {
            _bernsteinValues = bernsteinValues;
        }
        public ParametricBezierPoint() : base()
        {
            _bernsteinValues = new();
        }
    }
}
