using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparkyStudios.TrakLibrary.Controls.Common
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FullPageErrorControl
    {
        public static readonly BindableProperty ErrorMessageProperty = 
            BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(FullPageErrorControl));

        public static readonly BindableProperty IsRefreshableProperty =
            BindableProperty.Create(nameof(IsRefreshable), typeof(bool), typeof(FullPageErrorControl));
        
        public static readonly BindableProperty RefreshCommandProperty =
            BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(FullPageErrorControl));
        
        public FullPageErrorControl()
        {
            InitializeComponent();
        }

        public string ErrorMessage
        {
            get => (string) GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public bool IsRefreshable
        {
            get => (bool) GetValue(IsRefreshableProperty);
            set => SetValue(IsRefreshableProperty, value);
        }

        public ICommand RefreshCommand
        {
            get => (ICommand) GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }
    }
}