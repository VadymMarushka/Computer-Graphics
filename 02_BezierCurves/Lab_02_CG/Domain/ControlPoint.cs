using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Wpf.Ui.Controls;

namespace Lab_02_CG.Domain
{
    public partial class ControlPoint : ObservableValidator
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(-100, 100, ErrorMessage = "must be in [-100; 100] range")]
        private double _x;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(-100, 100, ErrorMessage = "must be in [-100; 100] range")]
        private double _y;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RoleName))]
        private bool _isAnchor;

        public string RoleName => IsAnchor ? "Anchor point" : "Direction point";

        public ControlPoint(string name, double x, double y, bool isAnchor = false)
        {
            Name = name;
            X = x;
            Y = y;
            IsAnchor = isAnchor;
            ErrorsChanged += ControlPoint_ErrorsChanged;
        }

        private void ControlPoint_ErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            var errors = GetErrors(e.PropertyName).ToList();

            if (errors.Any())
            {
                System.Windows.MessageBox.Show(
                    $"Error in {e.PropertyName}: {errors.First().ErrorMessage}",
                    "Validation Error",
                    System.Windows.MessageBoxButton.OK,
                    MessageBoxImage.Error);

                if (e.PropertyName == nameof(X)) X = 0;
                if (e.PropertyName == nameof(Y)) Y = 0;
            }
        }

    }
}
