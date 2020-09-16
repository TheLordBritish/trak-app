using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparkyStudios.TrakLibrary.Extensions
{
    [ContentProperty("Value")]
    public class GreaterThanOrEqualExtension : IMarkupExtension, IValueConverter
    {
        public int Value { get; set; }
        
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) >= Value;
        } 

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}