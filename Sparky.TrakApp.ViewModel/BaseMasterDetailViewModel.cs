using Prism.Navigation;

namespace Sparky.TrakApp.ViewModel
{
    public class BaseMasterDetailViewModel : BaseViewModel
    {
        public BaseMasterDetailViewModel(INavigationService navigationService) : base(navigationService)
        {
        }
    }
}