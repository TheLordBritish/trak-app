using System;
using System.Net;
using Prism.Commands;
using Prism.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentValidation;
using Plugin.FluentValidationRules;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Login
{
    public class LoginViewModel : BaseViewModel, IValidate<UserCredentials>
    {
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;
        
        private IValidator _validator;
        private Validatables _validatables;
        
        private Validatable<string> _username;
        private Validatable<string> _password;
        private string _errorMessage;

        public LoginViewModel(INavigationService navigationService, IAuthService authService, IStorageService storageService) : base(navigationService)
        {
            _authService = authService;
            _storageService = storageService;
            
            _username = new Validatable<string>(nameof(UserCredentials.Username));
            _password = new Validatable<string>(nameof(UserCredentials.Password));
            
            SetupForValidation();
        }

        public ICommand ClearValidationCommand => new DelegateCommand<string>(ClearValidation);

        public ICommand LoginCommand => new DelegateCommand(async () => await LoginAsync());

        public ICommand RegisterCommand => new DelegateCommand(async () => await RegisterAsync());
        
        public Validatable<string> Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public Validatable<string> Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public void SetupForValidation()
        {
            _validator = new UserCredentialsValidator();
            _validatables = new Validatables(Username, Password);
        }

        public OverallValidationResult Validate(UserCredentials model)
        {
            return _validator.Validate(model)
                .ApplyResultsTo(_validatables);
        }

        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }
        
        private async Task LoginAsync()
        {
            IsError = false;
            IsBusy = true;
            
            var registration = _validatables.Populate<UserCredentials>();
            var validationResult = Validate(registration);
            
            try
            {
                if (validationResult.IsValidOverall)
                {
                    var token = await _authService.GetTokenAsync(new UserCredentials
                    {
                        Username = Username.Value,
                        Password = Password.Value
                    });

                    // Store the needed credentials in the store.
                    await _storageService.SetAuthTokenAsync(token);
                    await _storageService.SetUsernameAsync(Username.Value);

                    if (!await _authService.IsVerifiedAsync(Username.Value, token))
                    {
                        await NavigationService.NavigateAsync("VerificationPage");
                    }
                    else
                    {
                        await NavigationService.NavigateAsync("/BaseMasterDetailPage/NavigationPage/HomePage");
                    }
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
