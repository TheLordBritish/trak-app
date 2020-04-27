using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentValidation;
using Plugin.FluentValidationRules;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;
using Xamarin.Essentials;

namespace Sparky.TrakApp.ViewModel.Login
{
    public class RegisterViewModel : BaseViewModel, IValidate<RegistrationDetails>
    {
        private readonly IAuthService _authService;
        
        private IValidator _validator;
        private Validatables _validatables;
        
        private Validatable<string> _username;
        private Validatable<string> _emailAddress;
        private Validatable<string> _password;
        private Validatable<string> _confirmPassword;
        private string _errorMessage;
        
        public RegisterViewModel(INavigationService navigationService, IAuthService authService) : base(navigationService)
        {
            _authService = authService;
            
            Username = new Validatable<string>(nameof(RegistrationDetails.Username));
            EmailAddress = new Validatable<string>(nameof(RegistrationDetails.EmailAddress));
            Password = new Validatable<string>(nameof(RegistrationDetails.Password));
            ConfirmPassword = new Validatable<string>(nameof(RegistrationDetails.ConfirmPassword));

            SetupForValidation();
        }
        
        public ICommand ClearValidationCommand => new DelegateCommand<string>(ClearValidation);
        
        public ICommand RegisterCommand => new DelegateCommand(async () => await RegisterAsync());

        public ICommand LoginCommand => new DelegateCommand(async () => await LoginAsync());

        public Validatable<string> Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public Validatable<string> EmailAddress
        {
            get => _emailAddress;
            set => SetProperty(ref _emailAddress, value);
        }

        public Validatable<string> Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public Validatable<string> ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public void SetupForValidation()
        {
            _validator = new RegistrationDetailsValidator();
            _validatables = new Validatables(Username, EmailAddress, Password, ConfirmPassword);
        }

        public OverallValidationResult Validate(RegistrationDetails model)
        {
            return _validator.Validate(model)
                .ApplyResultsTo(_validatables);
        }

        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }
        
        private async Task RegisterAsync()
        {
            IsBusy = true;
            
            var registration = _validatables.Populate<RegistrationDetails>();
            var validationResult = Validate(registration);

            if (validationResult.IsValidOverall)
            {
                try
                {
                    // Register the new account.
                    await _authService.RegisterAsync(new RegistrationRequest
                    {
                        Username = Username.Value,
                        EmailAddress = EmailAddress.Value,
                        Password = Password.Value
                    });

                    var token = await _authService.GetTokenAsync(new UserCredentials
                    {
                        Username = Username.Value,
                        Password = Password.Value
                    });
                    
                    // Store the needed credentials in the store.
                    await SecureStorage.SetAsync("auth-token", token);
                    await SecureStorage.SetAsync("username", Username.Value);
                    await SecureStorage.SetAsync("password", Password.Value);
                    
                    // Navigate to the verification page for the user to verify their account before use.
                    await NavigationService.NavigateAsync("VerificationPage");   
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
        
        private async Task LoginAsync()
        {
            await NavigationService.GoBackAsync();
        }
    }
}