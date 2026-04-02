using CommunityToolkit.Mvvm.ComponentModel;
using Lab_03_CG.MVVM.ViewModels;
using Lab_03_CG.MVVM.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Lab_03_CG
{
    public sealed partial class MainWindow : Window
    {
        private readonly IServiceProvider _services;
        public MainViewModel VM { get; }
        public MainWindow(MainViewModel viewModel, IServiceProvider services)
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            VM = viewModel;
            _services = services;
            NavigateTo("AlgebraicView");
            MainNavView.SelectedItem = NavAlgebraic;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string pageTag = selectedItem.Tag?.ToString();
            NavigateTo(pageTag);
        }

        private void NavigateTo(string pageTag)
        {
            var title = new StringBuilder("Fractal Graphics");
            if (pageTag == "AlgebraicView")
            {
                ContentFrame.Content = _services.GetRequiredService<AlgebraicView>();
                title.Append(" | Algebraic");
            }
            else if (pageTag == "GeometricView")
            {
                ContentFrame.Content = _services.GetRequiredService<GeometricView>();
                title.Append(" | Geometric");
            }
            else if (pageTag == "GalleryView")
            {
                ContentFrame.Content = _services.GetRequiredService<GalleryView>();
                title.Append(" | Gallery");
            }
            VM.Title = title.ToString();
        }
    }
}
