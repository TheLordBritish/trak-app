using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;

namespace Sparky.TrakApp.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(INavigationService navigationService) : base(navigationService)
        {
            LoadGamesCommand = new DelegateCommand(async () => await LoadGamesAsync());
        }
        
        public ICommand LoadGamesCommand { get; set; }

        private async Task LoadGamesAsync()
        {
            await NavigationService.NavigateAsync("GameUserEntriesTabbedPage");
        }
    }
}