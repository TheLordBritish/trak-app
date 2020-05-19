using System.Collections;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sparky.TrakApp.Controls
{
    public class InfiniteListView : ListView
    {
        public static readonly BindableProperty LoadMoreCommandProperty 
            = BindableProperty.Create(nameof(LoadMoreCommand), typeof(ICommand), typeof(ICommand));

        public InfiniteListView()
        {
            ItemAppearing += (sender, args) =>
            {
                if (ItemsSource is IList items && args.Item == items[items.Count - 1])
                {
                    if (LoadMoreCommand != null && LoadMoreCommand.CanExecute(null))
                    {
                        LoadMoreCommand.Execute(null);
                    }
                }
            };
        }
        
        public ICommand LoadMoreCommand
        {
            get => GetValue(LoadMoreCommandProperty) as ICommand;
            set => SetValue(LoadMoreCommandProperty, value);
        }
    }
}