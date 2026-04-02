using CommunityToolkit.Mvvm.ComponentModel;
using Lab_03_CG.MVVM.Model;
using System.Collections.ObjectModel;

namespace Lab_03_CG.MVVM.ViewModels.Dialogs
{
    public partial class AddFractalDialogViewModel : ObservableObject
    {
        [ObservableProperty] private string _imagePath = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSaveEnabled))]
        private string _title = string.Empty;

        [ObservableProperty] private string _description = string.Empty;

        public bool IsSaveEnabled => !string.IsNullOrWhiteSpace(Title);

        public ObservableCollection<string> ParameterSummary { get; } = new();

        [ObservableProperty] private byte[] _imageBytes;

        public static AddFractalDialogViewModel FromDTO(AddFractalDTO dto)
        {
            var vm = new AddFractalDialogViewModel
            {
                ImageBytes = dto.Image,
                Title = dto.SuggestedTitle,
            };

            switch (dto)
            {
                case AddAlgebraicFractalDTO a:
                    vm.ParameterSummary.Add($"Formula: {a.Formula}");
                    vm.ParameterSummary.Add($"Family: {a.Family}");
                    vm.ParameterSummary.Add($"Palette: {a.Palette}");
                    vm.ParameterSummary.Add($"MaxIter: {a.MaxIterations}");
                    vm.ParameterSummary.Add($"Escape: {a.EscapeRadius}");
                    vm.ParameterSummary.Add($"C: {a.C_Real:F3}+{a.C_Imaginary:F3}i");
                    break;

                case AddGeometricFractalDTO g:
                    vm.ParameterSummary.Add($"Axiom: {g.Initializer}");
                    vm.ParameterSummary.Add($"Rule: {g.Generator}");
                    vm.ParameterSummary.Add($"Size: {g.SideLength:F2}");
                    vm.ParameterSummary.Add($"Rotation: {g.RotationAngle}°");
                    vm.ParameterSummary.Add($"Color: {g.LineColor}");
                    vm.ParameterSummary.Add($"Center: ({g.CenterX:F2}, {g.CenterY:F2})");
                    break;
            }

            return vm;
        }

    }
}
