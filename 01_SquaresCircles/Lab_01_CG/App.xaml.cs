using Lab_01_CG.Data;
using Lab_01_CG.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace Lab_01_CG
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
            services.AddDbContextFactory<SquaresAndCirclesContext>
                (options => options.UseSqlite($"Data Source={dbPath}"));
            services.AddSingleton<MainWindow>();
            services.AddSingleton<SquareAndCirclesDataService>();
            services.AddSingleton<MainViewModel>();
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
