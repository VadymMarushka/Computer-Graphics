using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab_03_CG.Data.Entities;
using Lab_03_CG.MVVM.Model;
using Lab_03_CG.Services.Abstractions;
using Lab_03_CG.Services.Implementaions;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using ScottPlot;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Windows.Globalization.NumberFormatting;

namespace Lab_03_CG.MVVM.ViewModels
{
    public partial class AlgebraicViewModel : ObservableObject
    {
        [ObservableProperty] private DecimalFormatter _numberFormatter;

        private readonly IRainbowAnimationService _rainbowService;
        public SolidColorBrush RainbowBrush => _rainbowService.AnimatedBrush;
        public LinearGradientBrush RainbowGradientBrush => _rainbowService.AnimatedGradientBrush;

        [ObservableProperty] private int _selectedFormulaIndex = 0;
        [ObservableProperty] private double _constantReal = -0.7;
        [ObservableProperty] private double _constantImaginary = 0.27015;
        [ObservableProperty] private double _z0Real = 0.0;
        [ObservableProperty] private double _z0Imaginary = 0.0;
        [ObservableProperty] private double _maxIterations = 100;
        [ObservableProperty] private double _escapeRadius = 4.0;
        [ObservableProperty] private int _selectedPaletteIndex = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCEnabled))]
        [NotifyPropertyChangedFor(nameof(IsZ0Enabled))]
        [NotifyPropertyChangedFor(nameof(CLabel))]
        [NotifyPropertyChangedFor(nameof(Z0Label))]
        private int _selectedModeIndex = 0;

        public bool IsCEnabled => SelectedModeIndex == 2;
        public bool IsZ0Enabled => SelectedModeIndex == 1 || SelectedModeIndex == 3;

        public string CLabel => SelectedModeIndex switch
        {
            0 => "Enter C (From coordinates):",
            1 => "Enter C (From coordinates):",
            2 => "Enter C:",
            3 => "Enter C (Mirrored Coord):",
            _ => "Enter C:"
        };
        public string Z0Label => SelectedModeIndex switch
        {
            0 => "Enter Z₀ (Set to zero):",
            1 => "Enter Z₀:",
            2 => "Enter Z₀ (From coordinates):",
            3 => "Enter Z₀:",
            _ => "Enter Z₀:"
        };

        [ObservableProperty] private double _panX = 0.0;
        [ObservableProperty] private double _panY = 0.0;
        [ObservableProperty] private double _zoom = 1.0;
        public Func<Task<byte[]>>? CaptureImageBytesAsync { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotAnimating))]
        [NotifyPropertyChangedFor(nameof(IsChromaticActive))]
        private bool _chromaticMode = false;
        public bool IsChromaticActive => ChromaticMode;
        public bool IsNotAnimating => !IsChromaticActive;

        public IFractalDataService _fractalDataService;
        private readonly IDialogService _dialogService;

        [ObservableProperty] private double _chromaticProgress = 100;

        [ObservableProperty] private string _chromaticTimeLabel = "10s remaining";

        private const double ChromaticDuration = 10.0;
        private double _chromaticSecondsLeft;
        private DispatcherQueueTimer _chromaticTimer;

        public AlgebraicViewModel(IRainbowAnimationService rainbowService, IFractalDataService fractalDataService, IDialogService dialogService)
        {
            _rainbowService = rainbowService;
            NumberFormatter = new DecimalFormatter { FractionDigits = 3 };
            _rainbowService.Start();
            _fractalDataService = fractalDataService;
            this._dialogService = dialogService;
        }
        [RelayCommand] public void ExportAsPNG() => ExportRequested?.Invoke();

        public event Action? ExportRequested;

        [RelayCommand]
        public async Task SaveToGallery(object parameter)
        {
            if (parameter is not FrameworkElement { XamlRoot: not null } element) return;

            byte[] imgBytes = null;
            if (CaptureImageBytesAsync != null) imgBytes = await CaptureImageBytesAsync();

            var dto = new AddAlgebraicFractalDTO
            {
                Image = imgBytes,
                SuggestedTitle = "Algebraic Fractal",
                Formula = SelectedFormulaIndex switch
                {
                    0 => "Quadratic: z²+c",
                    1 => "Cubic: z³+c",
                    2 => "Sinusoidal: sin(z)+c",
                    _ => "Variant: tg(z²)+c",
                },
                Family = SelectedModeIndex switch
                {
                    0 => "Mandelbrot",
                    1 => "Custom Z₀",
                    2 => "Julia",
                    _ => "Mirrored",
                },
                Palette = SelectedPaletteIndex switch
                {
                    0 => "Grayscale",
                    1 => "Solar Eclipse",
                    2 => "Ship with Ghosts",
                    3 => "Arctic",
                    _ => "Glowstick",
                },
                Z0_Real = Z0Real,
                Z0_Imaginary = Z0Imaginary,
                C_Real = ConstantReal,
                C_Imaginary = ConstantImaginary,
                EscapeRadius = EscapeRadius,
                MaxIterations = (int)MaxIterations,
            };
            if (ChromaticMode) dto.Palette = "Chromatic";

            var fractal = await _dialogService.ShowAddFractalDialogAsync(element.XamlRoot, dto);

            if (fractal is not null)
            {
                await _fractalDataService.AddFractalAsync(fractal, dto.Image);
            }

        }
        [RelayCommand] public void ResetView() { PanX = 0; PanY = 0; Zoom = 1; }

        [RelayCommand]
        public void StartChromatic()
        {
            StopChromaticInternal();
            AppNotification notification = new AppNotificationBuilder()
            .AddText("📸 TAKE OUT THE CAMERA 📸")
            .AddText("Hurry up! This mesmerizing chromatic fractal is going to be here only for 10 seconds!")
            .BuildNotification();

            AppNotificationManager.Default.Show(notification);
            _chromaticSecondsLeft = ChromaticDuration;
            ChromaticProgress = 100;
            ChromaticMode = true;
            ChromaticTimeLabel = FormatTime(_chromaticSecondsLeft);

            var queue = DispatcherQueue.GetForCurrentThread();
            _chromaticTimer = queue.CreateTimer();
            _chromaticTimer.Interval = TimeSpan.FromMilliseconds(100);
            _chromaticTimer.Tick += (_, _) =>
            {
                _chromaticSecondsLeft -= 0.1;

                if (_chromaticSecondsLeft <= 0)
                {
                    StopChromaticInternal();
                    return;
                }

                ChromaticProgress = _chromaticSecondsLeft / ChromaticDuration * 100.0;
                ChromaticTimeLabel = FormatTime(_chromaticSecondsLeft);
            };
            _chromaticTimer.Start();
        }

        [RelayCommand]
        public void StopChromatic() => StopChromaticInternal();

        private void StopChromaticInternal()
        {
            _chromaticTimer?.Stop();
            _chromaticTimer = null;
            ChromaticProgress = 100;
            ChromaticMode = false;
            ChromaticTimeLabel = "10s remaining";
        }

        private static string FormatTime(double seconds)
        {
            int s = (int)Math.Ceiling(seconds);
            return $"{s}s remaining";
        }
    }
}