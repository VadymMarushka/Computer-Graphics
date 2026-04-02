using Lab_03_CG.Data;
using Lab_03_CG.MVVM.ViewModels;
using Lab_03_CG.MVVM.Views;
using Lab_03_CG.Services.Abstractions;
using Lab_03_CG.Services.Implementaions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Lab_03_CG
{
    public partial class App : Application
    {
        private IServiceProvider _services;
        public Window MainWindow;
        public App()
        {
            this.InitializeComponent();
            var services = new ServiceCollection();

            services.AddSingleton<IRainbowAnimationService, RainbowAnimationService>();

            services.AddTransient<AlgebraicViewModel>();
            services.AddTransient<AlgebraicView>();

            services.AddTransient<GeometricViewModel>();
            services.AddTransient<GeometricView>();

            services.AddTransient<GalleryView>();
            services.AddTransient<GalleryViewModel>();

            services.AddSingleton<MainWindow>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IExportService, ExportService>();
            services.AddSingleton<IDialogService, DialogService>();

            string basePath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            string folderPath = System.IO.Path.Combine(basePath, "Lab_03_CG");

            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            string dbPath = System.IO.Path.Combine(folderPath, "fractals.db");

            services.AddDbContextFactory<FractalsDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));
            services.AddSingleton<IFractalDataService, FractalDataService>();
            _services = services.BuildServiceProvider();
            using (var context = _services.GetRequiredService<IDbContextFactory<FractalsDbContext>>().CreateDbContext())
            {
                context.Database.EnsureCreated();
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = _services.GetRequiredService<MainWindow>();
            MainWindow.Activate();
        }
    }
}
