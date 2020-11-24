using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Views
{
    public partial class BaseFlyoutPage
    {
        public BaseFlyoutPage()
        {
            InitializeComponent();
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
        }
    }
}