using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            BackgroundImageSource = ImageSource.FromFile("background.png");
        }
    }
}