using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseNavigationPage
    {
        public BaseNavigationPage()
        {
            InitializeComponent();
        }
    }
}