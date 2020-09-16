using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparkyStudios.TrakLibrary.Extensions
{
    [ContentProperty("Text")]
    class TranslateExtension : IMarkupExtension
    {
        private const string _resourceId = "SparkyStudios.TrakLibrary.ViewModel.Resources.Messages";
        
        public string Text { get; set; }
        
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return string.Empty;
            }
            
            var resourceManager = new ResourceManager(_resourceId, typeof(ReactiveViewModel).GetTypeInfo().Assembly);
            var message = resourceManager.GetString(Text, CultureInfo.CurrentCulture);

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException($"Key: {Text} was not found in resources: {_resourceId} for culture: {CultureInfo.CurrentCulture.Name}");
            }

            return message;
        }
    }
}