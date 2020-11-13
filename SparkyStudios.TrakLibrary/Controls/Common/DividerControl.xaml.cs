using System.Collections.Generic;
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
        
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<ItemEntryViewModel>), typeof(DividerControl));
        
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
        
        public IEnumerable<ItemEntryViewModel> ItemsSource
        {
            get => (IEnumerable<ItemEntryViewModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
    }
}