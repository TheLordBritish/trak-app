using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Common;

namespace Sparky.TrakApp.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(INavigationService navigationService) : base(navigationService)
        {
            
        }
        
        public ICommand LoadGamesCommand => new DelegateCommand(async () => await LoadGamesAsync());

        private async Task LoadGamesAsync()
        {
            await NavigationService.NavigateAsync("GameUserEntriesTabbedPage", new NavigationParameters
            {
                {"transition-type", TransitionType.SlideFromRight}
            });
        }
    }
}