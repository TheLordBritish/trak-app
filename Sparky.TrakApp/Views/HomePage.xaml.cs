using Xamarin.Forms;

namespace Sparky.TrakApp.Views
{
    public partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
            BackgroundImageSource = ImageSource.FromFile("background.png");
        }
    }
}