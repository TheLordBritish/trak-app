using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Converters
{
    public class IEnumerableToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var values = (value as IEnumerable ?? new List<object>())
                .Cast<object>()
                .Select(v => v.ToString());

            return string.Join(parameter == null ? ", " : parameter.ToString(), values);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}