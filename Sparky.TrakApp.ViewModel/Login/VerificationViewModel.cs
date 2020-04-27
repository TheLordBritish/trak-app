using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Login
{
    public class VerificationViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;
        
        private short? _verificationCode;
        private string _errorMessage;
        
        public VerificationViewModel(INavigationService navigationService, IAuthService authService, IStorageService storageService) : base(navigationService)
        {
            _authService = authService;
            _storageService = storageService;
        }

        public ICommand VerifyCommand => new DelegateCommand(async () => await VerifyAsync());
        
        public short? VerificationCode
        {
            get => _verificationCode;
            set => SetProperty(ref _verificationCode, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private async Task VerifyAsync()
        {
            IsBusy = true;

            var username = await _storageService.GetUsernameAsync();
            var authToken = await _storageService.GetAuthTokenAsync();
            
            try
            {
                await _authService.VerifyAsync(username, VerificationCode.GetValueOrDefault(), authToken);
                await NavigationService.NavigateAsync("/NavigationPage/HomePage");
            }
            catch (ApiException e)
            {
                IsError = true;
                ErrorMessage = Messages.ErrorMessageApiError;
            }
            catch (Exception e)
            {
                IsError = true;
                ErrorMessage = Messages.ErrorMessageGeneric;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}