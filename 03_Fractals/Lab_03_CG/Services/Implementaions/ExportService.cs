using Lab_03_CG.Services.Abstractions;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Lab_03_CG.Services.Implementaions
{
    public class ExportService : IExportService
    {
        public async Task<string?> PickSavePathAsync(
            Window window, string suggestedName = "fractal")
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName      = suggestedName,
            };
            picker.FileTypeChoices.Add("PNG Image", new[] { ".png" });

            // WinUI 3 requires the picker to be initialised with the window handle.
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            return file?.Path;
        }
    }
}
