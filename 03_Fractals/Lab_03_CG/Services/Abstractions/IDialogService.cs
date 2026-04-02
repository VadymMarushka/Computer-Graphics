using Lab_03_CG.Data.Entities;
using Lab_03_CG.MVVM.Model;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_03_CG.Services.Abstractions
{
    public interface IDialogService
    {
        /// <summary>
        /// Shows the Add-to-Gallery dialog pre-filled from <paramref name="dto"/>.
        /// Returns the constructed Fractal entity if the user clicked Save,
        /// or null if they cancelled.
        /// </summary>
        public Task<Fractal?> ShowAddFractalDialogAsync(XamlRoot xamlRoot, AddFractalDTO dto);
    }
    
}
