using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace Lab_03_CG.Services.Abstractions
{
    public interface IExportService
    {
        /// <summary>
        /// Shows a FileSavePicker and returns the chosen path,
        /// or null if the user cancelled.
        /// </summary>
        Task<string?> PickSavePathAsync(Window window, string suggestedName = "fractal");
    }
}
