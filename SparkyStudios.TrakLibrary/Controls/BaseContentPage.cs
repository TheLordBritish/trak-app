using SparkyStudios.TrakLibrary.Common;
using Xamarin.Forms;

namespace SparkyStudios.TrakLibrary.Controls
{
    public class BaseContentPage : ContentPage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            MessagingCenter.Subscribe<object, TransitionType>(this, "PageTransition", (sender, args) =>
            {
                if (Parent is BaseNavigationPage navigationPage)
                {
                    navigationPage.TransitionType = args;
                }
            });
        }
    }
}