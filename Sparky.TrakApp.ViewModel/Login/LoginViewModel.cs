using System;
using System.Net;
using Prism.Commands;
using Prism.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;
using Xamarin.Essentials;

namespace Sparky.TrakApp.ViewModel.Login
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public LoginViewModel(INavigationService navigationService, IAuthService authService) : base(navigationService)
        {
            _authService = authService;
        }

        public ICommand LoginCommand => new DelegateCommand(async () => await LoginAsync());

        public ICommand RegisterCommand => new DelegateCommand(async () => await RegisterAsync());
        
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        private async Task LoginAsync()
        {
            IsError = false;
            IsBusy = true;
            
            try
            {
                var token = await _authService.GetTokenAsync(new UserCredentials
                {
                    Username = Username,
                    Password = Password
                });

                // Store the needed credentials in the store.
                await SecureStorage.SetAsync("auth-token", token);
                await SecureStorage.SetAsync("username", Username);
                await SecureStorage.SetAsync("password", Password);

                if (!await _authService.IsVerifiedAsync(Username, token))
                {
                    await NavigationService.NavigateAsync("VerificationPage");
                }
                else
                {
                    await NavigationService.NavigateAsync("/NavigationPage/HomePage");
                }
            }
            catch (ApiException e)
            {
                IsError = true;
                ErrorMessage = e.StatusCode == HttpStatusCode.Unauthorized ? Messages.ErrorMessageIncorrectCredentials : Messages.ErrorMessageApiError;
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

        private async Task RegisterAsync()
        {
            await NavigationService.NavigateAsync("RegisterPage");
        }
    }
}
