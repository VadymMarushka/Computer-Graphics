using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab_03_CG.Data.Entities;
using Lab_03_CG.Services.Abstractions;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lab_03_CG.MVVM.ViewModels
{
    public partial class GalleryViewModel : ObservableObject
    {
        private readonly IFractalDataService _fractalDataService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmpty))]
        [NotifyPropertyChangedFor(nameof(IsNotEmpty))]
        private ObservableCollection<FractalCardViewModel> _fractals = new();

        public Visibility IsEmpty => Fractals.Count == 0
                                         ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsNotEmpty => Fractals.Count > 0
                                         ? Visibility.Visible : Visibility.Collapsed;

        public GalleryViewModel(IFractalDataService fractalDataService)
        {
            _fractalDataService = fractalDataService;
            _ = LoadFractalsAsync();
        }

        [RelayCommand]
        public async Task LoadFractalsAsync()
        {
            var data = await _fractalDataService.GetAllFractalsAsync();
            Fractals.Clear();
            foreach (var f in data)
                Fractals.Add(new FractalCardViewModel(f));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsNotEmpty));
        }

        [RelayCommand]
        public async Task DeleteFractalAsync(FractalCardViewModel card)
        {
            if (card is null) return;

            await _fractalDataService.DeleteFractalAsync(card.Entity.Id);

            if (System.IO.File.Exists(card.Entity.Path))
            {
                try { System.IO.File.Delete(card.Entity.Path); } catch { }
            }

            Fractals.Remove(card);
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsNotEmpty));
        }
    }
}