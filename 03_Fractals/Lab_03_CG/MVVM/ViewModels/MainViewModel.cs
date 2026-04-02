using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_03_CG.MVVM.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = "Fractals";
        public MainViewModel()
        {

        }
    }
}
