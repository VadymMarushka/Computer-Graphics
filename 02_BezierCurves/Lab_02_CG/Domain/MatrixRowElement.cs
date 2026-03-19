using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_02_CG.Domain
{
    public partial class MatrixRowElement : ObservableObject
    {
        [ObservableProperty]
        private int _i;
        [ObservableProperty]
        private int _j;
        [ObservableProperty]
        private double _value;

        public MatrixRowElement(int i, int j, double value)
        {
            _i = i;
            _j = j;
            _value = value;
        }
        public MatrixRowElement()
        {
            _i = 0;
            _j = 0;
            _value = 0;
        }
    }
}
