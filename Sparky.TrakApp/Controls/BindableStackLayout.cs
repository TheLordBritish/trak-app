using System.Collections.Generic;
using Xamarin.Forms;

namespace Sparky.TrakApp.Controls
{
    public class BindableStackLayout : StackLayout
    {
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate),
            typeof(DataTemplate), typeof(BindableStackLayout), default, propertyChanged: OnItemTemplateChanged);

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource),
            typeof(IEnumerable<object>), typeof(BindableStackLayout), propertyChanged: OnItemsSourceChanged);

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public IEnumerable<object> ItemsSource
        {
            get => (IEnumerable<object>) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        
        private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != null)
            {
                (bindable as BindableStackLayout)?.PopulateItems();
            }
        }
        
        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != null)
            {
                (bindable as BindableStackLayout)?.PopulateItems();
            }
        }

        private void PopulateItems()
        {
            var items = ItemsSource;
            if (items == null || ItemTemplate == null)
            {
                return;
            }

            var children = Children;
            children.Clear();

            foreach (var item in items)
            {
                children.Add(InflateView(item));
            }
        }

        private View InflateView(object viewModel)
        {
            var view = (View) CreateContent(ItemTemplate, viewModel, this);
            view.BindingContext = viewModel;
            return view;
        }

        private static object CreateContent(DataTemplate template, object item, BindableObject container)
        {
            if (template is DataTemplateSelector selector)
            {
                template = selector.SelectTemplate(item, container);
            }

            var content = template.CreateContent();
            return content is View view ? view : ((ViewCell) content).View;
        }
    }
}