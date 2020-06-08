using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForgottenPasswordPage
    {
        public ForgottenPasswordPage()
        {
            InitializeComponent();
            BackgroundImageSource = ImageSource.FromFile("background.png");
        }
    }
}