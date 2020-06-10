using Xamarin.Forms;

namespace Sparky.TrakApp.Views.Login
{
    public partial class LoadingPage
    {
        public LoadingPage()
        {
            InitializeComponent();
            BackgroundImageSource = ImageSource.FromFile("background.png");
        }
    }
}