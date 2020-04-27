using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Xamarin.Essentials;

namespace Sparky.TrakApp.ViewModel.Login
{
    public class VerificationViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private short _verificationCode;
        
        public VerificationViewModel(INavigationService navigationService, IAuthService authService) : base(navigationService)
        {
            _authService = authService;
        }

        public ICommand VerifyCommand => new DelegateCommand(async () => await VerifyAsync());
        
        public short VerificationCode
        {
            get => _verificationCode;
            set => SetProperty(ref _verificationCode, value);
        }

        private async Task VerifyAsync()
        {
            IsBusy = true;
            
            var username = await SecureStorage.GetAsync("username");
            var authToken = await SecureStorage.GetAsync("auth-token");
            
            try
            {
                await _authService.VerifyAsync(username, VerificationCode, authToken);
                await NavigationService.NavigateAsync("/NavigationPage/HomePage");
            }
            catch (ApiException e)
            {
                // TODO:
            }
            catch (Exception e)
            {
                // TODO:
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}