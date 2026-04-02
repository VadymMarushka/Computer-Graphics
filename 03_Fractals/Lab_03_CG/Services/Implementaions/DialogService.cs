using Lab_03_CG.Data.Entities;
using Lab_03_CG.MVVM.Model;
using Lab_03_CG.MVVM.Views.Dialogs;
using Lab_03_CG.Services.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_03_CG.Services.Implementaions
{
    public class DialogService : IDialogService
    {
        public async Task<Fractal?> ShowAddFractalDialogAsync(
            XamlRoot xamlRoot, AddFractalDTO dto)
        {
            var dialog = new AddFractalDialog(dto)
            {
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();

            // ContentDialogResult.Primary == "Save" button
            return result == ContentDialogResult.Primary
                ? dialog.Result
                : null;
        }
    }
}
