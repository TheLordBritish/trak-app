using System;
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
        private readonly IRestService _restService;
        
        public BaseMasterDetailViewModel(INavigationService navigationService, IStorageService storageService, IRestService restService) : base(navigationService)
        {
            _storageService = storageService;
            _restService = restService;
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
            // Get some values first before removing them all from the secure storage.
            var userId = await _storageService.GetUserIdAsync();
            var deviceId = await _storageService.GetDeviceIdAsync();
            var token = await _storageService.GetAuthTokenAsync();
            
            // Remove all of the identifiable information from the secure store.
            await _storageService.SetUsernameAsync(string.Empty);
            await _storageService.SetPasswordAsync(string.Empty);
            await _storageService.SetAuthTokenAsync(string.Empty);
            await _storageService.SetUserIdAsync(0);

            try
            {
                // Need to ensure the correct details are registered for push notifications.
                await _restService.DeleteAsync($"api/notification-management/v1/notifications/unregister?user-id={userId}&device-guid={deviceId}", token);
            }
            catch (Exception)
            {
                // There's not much we can do if it fails, so just carry on with the standard logout process.
            }
            
            // Navigate back to the login page.
            await NavigationService.NavigateAsync("/LoginPage");
        }
    }
}