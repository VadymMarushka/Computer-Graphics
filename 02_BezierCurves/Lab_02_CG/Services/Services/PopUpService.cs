using Lab_02_CG.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Media;
using System.Text;
using Wpf.Ui;

namespace Lab_02_CG.Services.Services
{
    public class PopUpService : IPopUpService
    {
        private readonly IContentDialogService _contentDialogService;

        public PopUpService(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;
        }
        public async Task CallErrorMessageBox(string title, string message)
        {
            var contentPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
            };


            var errorIcon = new Wpf.Ui.Controls.SymbolIcon
            {
                Symbol = Wpf.Ui.Controls.SymbolRegular.ErrorCircle24,
                FontSize = 48,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 0, 16, 0),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = message,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                MaxWidth = 350
            };

            contentPanel.Children.Add(errorIcon);
            contentPanel.Children.Add(textBlock);

            var errorBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = title,
                Content = contentPanel,

                PrimaryButtonText = "OK",
                PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger,

                CloseButtonText = "Cancel",
                CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Secondary,

                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
            };
            SystemSounds.Hand.Play();
            await errorBox.ShowDialogAsync();
        }
        public async Task<Wpf.Ui.Controls.ContentDialogResult> CallQuestionContentDialog(string title, string message)
        {
            var contentPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
            };

            var warningIcon = new Wpf.Ui.Controls.SymbolIcon
            {
                Symbol = Wpf.Ui.Controls.SymbolRegular.Delete24,
                FontSize = 48,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 0, 16, 0),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = message,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                MaxWidth = 350
            };

            contentPanel.Children.Add(warningIcon);
            contentPanel.Children.Add(textBlock);

            var contentDialog = new Wpf.Ui.Controls.ContentDialog
            {
                Title = title,
                Content = contentPanel,
                PrimaryButtonText = "Delete Everything",
                PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger,
                CloseButtonText = "Cancel",
                DefaultButton = Wpf.Ui.Controls.ContentDialogButton.Close
            };

            SystemSounds.Exclamation.Play();
            return await _contentDialogService.ShowAsync(contentDialog, default);
        }
    }
}
