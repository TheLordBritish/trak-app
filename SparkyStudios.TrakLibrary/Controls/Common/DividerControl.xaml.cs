using System.Collections.Generic;
using System.Linq;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Controls.Common
{
    public partial class DividerControl
    {
        public static readonly BindableProperty DividerProperty =
            BindableProperty.Create(nameof(Divider), typeof(string), typeof(DividerControl));
        
        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(Divider), typeof(double), typeof(DividerControl), 14.0D);
        
        public static readonly BindableProperty LimitProperty =
            BindableProperty.Create(nameof(Limit), typeof(int), typeof(DividerControl), int.MaxValue);
        
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<ItemEntryViewModel>), typeof(DividerControl), propertyChanged: OnItemsSourceChanged);

        private IEnumerable<ItemEntryViewModel> _limitedItemsSource;

        public DividerControl()
        {
            InitializeComponent();
        }
        
        public string Divider
        {
            get => (string)GetValue(DividerProperty);
            set => SetValue(DividerProperty, value);
        }

        public double FontSize
        {
            get => (double) GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public int Limit
        {
            get => (int) GetValue(LimitProperty);
            set => SetValue(LimitProperty, value);
        }

        private static void OnItemsSourceChanged(BindableObject bindableObject, object oldValue, object newValue)
        {
            var dividerControl = (DividerControl) bindableObject;
            var limit = dividerControl.Limit;

            IList<ItemEntryViewModel> items = ((IEnumerable<ItemEntryViewModel>) newValue)
                ?.Take(limit)
                .ToList();

            var allItems = (IEnumerable<ItemEntryViewModel>) newValue;
            if (allItems != null && allItems.Count() > limit)
            {
                items.Add(new ItemEntryViewModel
                {
                    Name = "..."
                });
            }

            dividerControl.LimitedItemsSource = items;
        }
        
        public IEnumerable<ItemEntryViewModel> ItemsSource
        {
            get => (IEnumerable<ItemEntryViewModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public IEnumerable<ItemEntryViewModel> LimitedItemsSource
        {
            get => _limitedItemsSource;
            set
            {
                _limitedItemsSource = value;
                OnPropertyChanged(nameof(LimitedItemsSource));
            }
        }
    }
}