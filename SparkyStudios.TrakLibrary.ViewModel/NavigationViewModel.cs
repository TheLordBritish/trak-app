using System.Reactive.Concurrency;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.ViewModel.Common;

namespace SparkyStudios.TrakLibrary.ViewModel
{
    public class NavigationViewModel : ReactiveViewModel
    {
        public NavigationViewModel(IScheduler scheduler, INavigationService navigationService) : base(scheduler, navigationService)
        {
        }
    }
}