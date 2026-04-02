using Lab_03_CG.MVVM.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Lab_03_CG.MVVM.Views
{
    public sealed partial class GalleryView : Page
    {
        public GalleryViewModel VM { get; }

        public GalleryView(GalleryViewModel viewModel)
        {
            InitializeComponent();
            VM = viewModel;
        }
    }
}