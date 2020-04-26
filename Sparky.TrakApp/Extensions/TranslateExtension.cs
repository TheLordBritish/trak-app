using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Sparky.TrakApp.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp.Extensions
{
    [ContentProperty("Text")]
    class TranslateExtension : IMarkupExtension
    {
        private const string _resourceId = "Sparky.TrakApp.ViewModel.Resources.Messages";
        
        public string Text { get; set; }
        
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return string.Empty;
            }
            
            var resourceManager = new ResourceManager(_resourceId, typeof(BaseViewModel).GetTypeInfo().Assembly);
            var message = resourceManager.GetString(Text, CultureInfo.CurrentCulture);

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException($"Key: {Text} was not found in resources: {_resourceId} for culture: {CultureInfo.CurrentCulture.Name}");
            }

            return message;
        }
    }
}