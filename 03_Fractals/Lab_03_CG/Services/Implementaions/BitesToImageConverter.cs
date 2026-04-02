using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_03_CG.Services.Implementaions
{
    public class BytesToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                var bitmap = new BitmapImage();
                using var stream = new MemoryStream(bytes);
                bitmap.SetSource(stream.AsRandomAccessStream());
                return bitmap;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
