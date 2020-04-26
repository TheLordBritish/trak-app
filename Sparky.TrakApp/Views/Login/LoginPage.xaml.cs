using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BackgroundImageSource = ImageSource.FromFile("background.png");
        }
    }
}
