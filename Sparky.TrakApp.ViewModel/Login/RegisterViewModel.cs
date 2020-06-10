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

namespace Sparky.TrakApp.ViewModel.Login
{
    /// <summary>
    /// The <see cref="RegisterViewModel"/> is a simple view model that is associated with the register page view.
    /// Its responsibility is to respond to registration attempts made with credential information.
    ///
    /// The <see cref="RegisterViewModel"/> also provides methods to validate fields on the register page view. Any
    /// validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class RegisterViewModel : BaseViewModel, IValidate<RegistrationDetails>
    {
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;

        private IValidator _validator;
        private Validatables _validatables;

        private Validatable<string> _username;
        private Validatable<string> _emailAddress;
        private Validatable<string> _password;
        private Validatable<string> _confirmPassword;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="authService">The <see cref="IAuthService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        public RegisterViewModel(INavigationService navigationService, IAuthService authService,
            IStorageService storageService) : base(navigationService)
        {
            _authService = authService;
            _storageService = storageService;

            Username = new Validatable<string>(nameof(RegistrationDetails.Username));
            EmailAddress = new Validatable<string>(nameof(RegistrationDetails.EmailAddress));
            Password = new Validatable<string>(nameof(RegistrationDetails.Password));
            ConfirmPassword = new Validatable<string>(nameof(RegistrationDetails.ConfirmPassword));

            SetupForValidation();
        }

        /// <summary>
        /// Command that is invoked each time that a validatable field on the view is changed, which
        /// for the <see cref="RegisterViewModel"/> is the username, email address, password and confirm
        /// password code. When the view is changed, the name is passed through and the request propagated
        /// to the <see cref="ClearValidation"/> method.
        /// </summary>
        public ICommand ClearValidationCommand => new DelegateCommand<string>(ClearValidation);

        /// <summary>
        /// Command that is invoked by the view when the register button is tapped. When called, the command
        /// will propagate the request and call the <see cref="RegisterAsync"/> method.
        /// </summary>
        public ICommand RegisterCommand => new DelegateCommand(async () => await RegisterAsync());

        /// <summary>
        /// Command that is invoked by the view when the log in label is tapped. When called, the command
        /// will propagate the request and call the <see cref="LoginAsync"/> method.
        /// </summary>
        public ICommand LoginCommand => new DelegateCommand(async () => await LoginAsync());

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated username with
        /// additional validation information.
        /// </summary>
        public Validatable<string> Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated email address with
        /// additional validation information.
        /// </summary>
        public Validatable<string> EmailAddress
        {
            get => _emailAddress;
            set => SetProperty(ref _emailAddress, value);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated password with
        /// additional validation information.
        /// </summary>
        public Validatable<string> Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated password confirmation with
        /// additional validation information.
        /// </summary>
        public Validatable<string> ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        /// <summary>
        /// Invoked within the constructor of the <see cref="RegisterViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="RegistrationDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            _validator = new RegistrationDetailsValidator();
            _validatables = new Validatables(Username, EmailAddress, Password, ConfirmPassword);
        }

        /// <summary>
        /// Validates the specified <see cref="RegistrationDetails"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="RegistrationDetailsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="RegistrationDetails"/> instance to validate against the <see cref="RegistrationDetailsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(RegistrationDetails model)
        {
            return _validator.Validate(model)
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="RegisterViewModel"/>.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="RegisterCommand"/> when activated by the associated
        /// view. This method will validate the different member variables within this view model, the username, email
        /// address and password supplied by the view, and if valid attempt to register a new account with the provided
        /// information. If registration is successful, the user will be navigated to the verification page.
        ///
        /// If any errors occur during registration, the exceptions are caught and the errors are
        /// displayed to the user through the ErrorMessage parameter and setting the IsError boolean to true. The same
        /// is done if the calls were successful but registration failed for other reasons, such as accounts already
        /// being registered with the supplied details.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task RegisterAsync()
        {
            IsError = false;
            IsBusy = true;

            var registration = _validatables.Populate<RegistrationDetails>();
            var validationResult = Validate(registration);

            try
            {
                if (validationResult.IsValidOverall)
                {
                    await AttemptRegistrationAsync(Username.Value, EmailAddress.Value, Password.Value);
                }
            }
            catch (ApiException)
            {
                IsError = true;
                ErrorMessage = Messages.ErrorMessageApiError;
            }
            catch (Exception)
            {
                IsError = true;
                ErrorMessage = Messages.ErrorMessageGeneric;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Private method that is invoked within the <see cref="RegisterAsync"/> method. Its purpose
        /// is to attempt to register a new user with the supplied information by calling off to an external
        /// service, before storing some small amount of information for later use. 
        ///
        /// If the user entered valid registration information that isn't already in use, then they will be navigated
        /// to the verification page. However, if the user entered information that is already in use, then an
        /// error message stating that it is already in use will be presented to the user.
        /// </summary>
        /// <param name="username">The username to attempt registration with.</param>
        /// <param name="emailAddress">The email address to attempt registration with.</param>
        /// <param name="password">The password to attempt registration with.</param>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AttemptRegistrationAsync(string username, string emailAddress, string password)
        {
            // Attempt to register the new account.
            var userCreationResponse = await _authService.RegisterAsync(new RegistrationRequest
            {
                Username = username,
                EmailAddress = emailAddress,
                Password = password
            });

            // If there are errors with the registration, display them to the user.
            if (userCreationResponse.Error)
            {
                IsError = true;
                ErrorMessage = userCreationResponse.ErrorMessage;
            }
            else
            {
                // If there are no issues, retrieve the authenticated token.
                var user = userCreationResponse.Data;
                var token = await _authService.GetTokenAsync(new UserCredentials
                {
                    Username = user.Username,
                    Password = Password.Value
                });

                // Store the needed credentials in the store.
                await _storageService.SetAuthTokenAsync(token);
                await _storageService.SetUserIdAsync(user.Id);
                await _storageService.SetUsernameAsync(user.Username);
                await _storageService.SetPasswordAsync(password);

                // Navigate to the verification page for the user to verify their account before use.
                await NavigationService.NavigateAsync("VerificationPage");
            }
        }

        /// <summary>
        /// Invoked when the user <see cref="LoginCommand"/> is invoked by the view. All the method will do is
        /// navigate back to the login page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoginAsync()
        {
            await NavigationService.GoBackAsync();
        }
    }
}