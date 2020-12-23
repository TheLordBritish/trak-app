using System.Collections.Generic;
using System.Windows.Input;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Controls.Common
{
    public partial class FilterControl
    {
        public static readonly BindableProperty HeaderProperty =
            BindableProperty.Create(nameof(Header), typeof(string), typeof(FilterControl), string.Empty);
        
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<ItemEntryViewModel>), typeof(FilterControl));
        
        public static readonly BindableProperty GroupTappedCommandProperty =
            BindableProperty.Create(nameof(GroupTappedCommand), typeof(ICommand), typeof(FilterControl));
        
        public static readonly BindableProperty ItemTappedCommandProperty =
            BindableProperty.Create(nameof(ItemTappedCommand), typeof(ICommand), typeof(FilterControl));
        
        public static readonly BindableProperty IsGroupExpandedProperty =
            BindableProperty.Create(nameof(IsGroupExpanded), typeof(bool), typeof(FilterControl));
        
        public FilterControl()
        {
            InitializeComponent();
        }

        public string Header
        {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        
        public IEnumerable<ItemEntryViewModel> ItemsSource
        {
            get => (IEnumerable<ItemEntryViewModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ICommand GroupTappedCommand
        {
            get => (ICommand)GetValue(GroupTappedCommandProperty);
            set => SetValue(GroupTappedCommandProperty, value);
        }

        public ICommand ItemTappedCommand
        {
            get => (ICommand) GetValue(ItemTappedCommandProperty);
            set => SetValue(ItemTappedCommandProperty, value);
        }
        
        public bool IsGroupExpanded
        {
            get => (bool) GetValue(IsGroupExpandedProperty);
            set => SetValue(IsGroupExpandedProperty, value);
        }
    }
}