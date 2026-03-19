using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_02_CG.Services.Models
{
    public partial class BezierResponse<TBezierPoint> : ObservableObject
        where TBezierPoint : BezierPoint 
    {
        [ObservableProperty]
        private TBezierPoint[] _points;

        public BezierResponse(TBezierPoint[] points)
        {
            _points = points;
        }
        public BezierResponse()
        {
            _points = new TBezierPoint[0];
        }
    }
    public partial class BezierPoint : ObservableObject
    {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private double _x;
        [ObservableProperty]
        private double _y;
        [ObservableProperty]
        private double _t;

        public BezierPoint(int id, double x, double y, double t)
        {
            _id = id;
            _x = x;
            _y = y;
            _t = t;
        }
        public BezierPoint()
        {
            _id = 1;
            _x = 0;
            _y = 0;
            _t = 0;
        }
    }
}
