using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PharmacyApp.Converters
{
    public class ExpiryToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Transparent;

            if (!int.TryParse(parameter?.ToString(), out var days))
                days = 30;

            if (value is DateTime dt)
            {
                var today = DateTime.Today;
                var limit = today.AddDays(days);

                if (dt < today) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C53030"));
                if (dt <= limit) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDF2F7"));
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
