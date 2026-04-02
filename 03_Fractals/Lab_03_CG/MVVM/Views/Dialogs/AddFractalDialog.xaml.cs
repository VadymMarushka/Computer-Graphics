using Lab_03_CG.Data.Entities;
using Lab_03_CG.MVVM.Model;
using Lab_03_CG.MVVM.ViewModels.Dialogs;
using Microsoft.UI.Xaml.Controls;

namespace Lab_03_CG.MVVM.Views.Dialogs
{
    public sealed partial class AddFractalDialog : ContentDialog
    {
        public AddFractalDialogViewModel VM { get; }
        public Fractal? Result { get; private set; }

        private readonly AddFractalDTO _dto;

        public AddFractalDialog(AddFractalDTO dto)
        {
            _dto = dto;
            VM = AddFractalDialogViewModel.FromDTO(dto);

            InitializeComponent();
            PrimaryButtonClick += (_, _) => Result = BuildEntity();
        }

        private Fractal BuildEntity()
        {
            switch (_dto)
            {
                case AddAlgebraicFractalDTO a:
                    return new AlgebraicFractal
                    {
                        Title = VM.Title.Trim(),
                        Description = VM.Description.Trim(),
                        Formula = a.Formula,
                        Family = a.Family,
                        Palette = a.Palette,
                        Z0_Real = a.Z0_Real,
                        Z0_Imaginary = a.Z0_Imaginary,
                        C_Real = a.C_Real,
                        C_Imaginary = a.C_Imaginary,
                        EscapeRadius = a.EscapeRadius,
                        MaxIterations = a.MaxIterations,
                    };

                case AddGeometricFractalDTO g:
                    return new GeometricalFractal
                    {
                        Title = VM.Title.Trim(),
                        Description = VM.Description.Trim(),
                        Initializer = g.Initializer,
                        Generator = g.Generator,
                        CenterX = g.CenterX,
                        CenterY = g.CenterY,
                        SideLength = g.SideLength,
                        RotationAngle = g.RotationAngle,
                        LineColor = g.LineColor,
                    };

                default:
                    throw new System.InvalidOperationException(
                        $"Unknown DTO type: {_dto.GetType().Name}");
            }
        }
    }
}