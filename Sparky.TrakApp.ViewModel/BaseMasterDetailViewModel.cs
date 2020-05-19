using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;

namespace Sparky.TrakApp.ViewModel
{
    public class BaseMasterDetailViewModel : BaseViewModel
    {
        public BaseMasterDetailViewModel(INavigationService navigationService) : base(navigationService)
        {
            LoadHomeCommand = new DelegateCommand(async () => await LoadHomeAsync());
            LoadGamesCommand = new DelegateCommand(async () => await LoadGamesAsync());
        }
        
        public ICommand LoadHomeCommand { get; set; }
        
        public ICommand LoadGamesCommand { get; set; }
        
        private async Task LoadHomeAsync()
        {
            await NavigationService.NavigateAsync("BaseNavigationPage/HomePage");
        }
        
        private async Task LoadGamesAsync()
        {
            await NavigationService.NavigateAsync("BaseNavigationPage/GameUserEntriesTabbedPage");
        }
    }
}