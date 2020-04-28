using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sparky.TrakApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseMasterDetailContentPage
    {
        public BaseMasterDetailContentPage()
        {
            InitializeComponent();
            BackgroundImageSource = ImageSource.FromFile("background.png");
        }
    }
}