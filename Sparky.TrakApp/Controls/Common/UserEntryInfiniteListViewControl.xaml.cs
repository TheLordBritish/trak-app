using System.Collections;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp.Controls.Common
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserEntryInfiniteListViewControl
    {
        public static readonly BindableProperty IsRefreshingProperty = 
            BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(UserEntryInfiniteListViewControl));
        
        public static readonly BindableProperty ItemsSourceProperty = 
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(UserEntryInfiniteListViewControl));
        
        public static readonly BindableProperty LoadMoreCommandProperty =
            BindableProperty.Create(nameof(LoadMoreCommand), typeof(ICommand), typeof(UserEntryInfiniteListViewControl));
        
        public static readonly BindableProperty RefreshCommandProperty =
            BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(UserEntryInfiniteListViewControl));
        
        public UserEntryInfiniteListViewControl()
        {
            InitializeComponent();
        }

        public bool IsRefreshing
        {
            get => (bool) GetValue(IsRefreshingProperty);
            set => SetValue(IsRefreshingProperty, value);
        }
        
        public IEnumerable ItemsSource 
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ICommand LoadMoreCommand
        {
            get => (ICommand) GetValue(LoadMoreCommandProperty);
            set => SetValue(LoadMoreCommandProperty, value);
        }
        
        public ICommand RefreshCommand
        {
            get => (ICommand) GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }
    }
}