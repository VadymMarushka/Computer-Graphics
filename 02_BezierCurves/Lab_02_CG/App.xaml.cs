using Lab_02_CG.MVVM.ViewModels;
using Lab_02_CG.MVVM.Views;
using Lab_02_CG.Services.Abstractions;
using Lab_02_CG.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace Lab_02_CG
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider serviceProvider;
        public App()
        {
            var services = new ServiceCollection();
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dataBase.db");
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();
            services.AddNavigationViewPageProvider();

            services.AddSingleton<EditorViewModel>();
            services.AddSingleton<ExperimentsViewModel>();
            services.AddSingleton<IBezierService, BezierService>();
            services.AddSingleton<IPopUpService, PopUpService>();

            services.AddTransient<EditorView>();
            services.AddTransient<ExperimentsView>();


            serviceProvider = services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }

}
