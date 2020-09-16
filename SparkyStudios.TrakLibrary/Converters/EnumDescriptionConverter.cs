using System;
using System.ComponentModel;
using System.Globalization;
using SparkyStudios.TrakLibrary.Common.Extensions;
using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum v)
            {
                return v.GetAttributeValue<DescriptionAttribute, string>(x => x.Description);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}