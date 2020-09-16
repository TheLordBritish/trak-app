using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Views.Login
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