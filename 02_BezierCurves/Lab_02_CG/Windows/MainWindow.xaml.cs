using Lab_02_CG.MVVM.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace Lab_02_CG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow(
                MainViewModel viewModel,
                INavigationService navigationService,
                IServiceProvider serviceProvider,
                INavigationViewPageProvider pageService,
                IContentDialogService contentDialogService)
        {
            InitializeComponent();
            DataContext = viewModel;
            RootNavigation.SetServiceProvider(serviceProvider);
            RootNavigation.SetPageProviderService(pageService);


            navigationService.SetNavigationControl(RootNavigation);

            RootNavigation.Navigated += (sender, args) =>
            {
                string pageName = args.Page switch
                {
                    MVVM.Views.EditorView => " | Editor",
                    MVVM.Views.ExperimentsView => " | Experiments",
                    _ => ""
                };

                viewModel.Title = "Bézier Curves" + pageName;
            };

            Loaded += (_, _) => navigationService.Navigate(typeof(MVVM.Views.EditorView));
            contentDialogService.SetDialogHost(RootContentDialogHost);
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}