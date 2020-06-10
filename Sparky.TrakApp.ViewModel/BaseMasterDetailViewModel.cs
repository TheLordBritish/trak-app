using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Service;

namespace Sparky.TrakApp.ViewModel
{
    public class BaseMasterDetailViewModel : BaseViewModel
    {
        private readonly IStorageService _storageService;
        
        public BaseMasterDetailViewModel(INavigationService navigationService, IStorageService storageService) : base(navigationService)
        {
            _storageService = storageService;
        }
        
        public ICommand LoadHomeCommand => new DelegateCommand(async () => await LoadHomeAsync());
        
        public ICommand LoadGamesCommand => new DelegateCommand(async () => await LoadGamesAsync());
        
        public ICommand LogoutCommand => new DelegateCommand(async () => await LogoutAsync());
         
        private async Task LoadHomeAsync()
        {
            await NavigationService.NavigateAsync("BaseNavigationPage/HomePage");
        }
        
        private async Task LoadGamesAsync()
        {
            await NavigationService.NavigateAsync("BaseNavigationPage/GameUserEntriesTabbedPage");
        }

        private async Task LogoutAsync()
        {
            // Remove all of the identifiable information from the secure store.
            await _storageService.SetUsernameAsync(string.Empty);
            await _storageService.SetPasswordAsync(string.Empty);
            await _storageService.SetAuthTokenAsync(string.Empty);
            await _storageService.SetUserIdAsync(0);
            
            // Navigate back to the login page.
            await NavigationService.NavigateAsync("/LoginPage");
        }
    }
}