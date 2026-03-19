using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_02_CG.Services.Abstractions
{
    public interface IPopUpService
    {
        public Task CallErrorMessageBox(string title, string message);
        public Task<Wpf.Ui.Controls.ContentDialogResult> CallQuestionContentDialog(string title, string message);
    }
}
