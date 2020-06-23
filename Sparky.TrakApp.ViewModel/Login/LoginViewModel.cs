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
    /// <summary>
    /// The <see cref="LoginViewModel"/> is a simple view model that is associated with the login page view.
    /// Its responsibility is to respond to login attempts made with a username and password and to allow the user
    /// a way to navigate to the registration page.
    ///
    /// The <see cref="LoginViewModel"/> also provides methods to validate fields on the login page view. Any
    /// validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class LoginViewModel : BaseViewModel, IValidate<UserCredentials>
    {
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;

        private IValidator _validator;
        private Validatables _validatables;

        private Validatable<string> _username;
        private Validatable<string> _password;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="authService">The <see cref="IAuthService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        public LoginViewModel(INavigationService navigationService, IAuthService authService,
            IStorageService storageService, IRestService restService)
            : base(navigationService)
        {
            _authService = authService;
            _storageService = storageService;
            _restService = restService;
            
            _username = new Validatable<string>(nameof(UserCredentials.Username));
            _password = new Validatable<string>(nameof(UserCredentials.Password));

            SetupForValidation();
        }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="LoginViewModel"/> is the username and password. When the view is changed,
        /// the name is passed through and the request propagated to the <see cref="ClearValidation"/>
        /// methods.
        /// </summary>
        public ICommand ClearValidationCommand => new DelegateCommand<string>(ClearValidation);

        /// <summary>
        /// Command that is invoked by the view when the login button is tapped. When called, the command
        /// will propagate the request and call the <see cref="LoginAsync"/> method.
        /// </summary>
        public ICommand LoginCommand => new DelegateCommand(async () => await LoginAsync());

        /// <summary>
        /// Command that is invoked by the view when the forgotten password label is tapped. When called,
        /// the command will propagate the request and call the <see cref="ForgottenPasswordAsync"/> method.
        /// </summary>
        public ICommand ForgottenPasswordCommand => new DelegateCommand(async () => await ForgottenPasswordAsync());
        
        /// <summary>
        /// Command that is invoked by the view when the register label is tapped. When called, the command
        /// will propagate the request and call the <see cref="RegisterAsync"/> method.
        /// </summary>
        public ICommand RegisterCommand => new DelegateCommand(async () => await RegisterAsync());

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
        /// A <see cref="Validatable{T}"/> that contains the currently populated password with
        /// additional validation information.
        /// </summary>
        public Validatable<string> Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        /// <summary>
        /// Invoked within the constructor of the <see cref="LoginViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="UserCredentialsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            _validator = new UserCredentialsValidator();
            _validatables = new Validatables(Username, Password);
        }

        /// <summary>
        /// Validates the specified <see cref="UserCredentials"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="UserCredentialsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="UserCredentials"/> instance to validate against the <see cref="UserCredentialsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(UserCredentials model)
        {
            return _validator.Validate(model)
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="LoginViewModel"/>.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoginCommand"/> when activated by the associated
        /// view. This method will validate the username and password on the view, and if valid attempt to retrieve
        /// and store authentication information for the user before navigating them to either the home page or the
        /// verification page, if they have yet to validate their account.
        ///
        /// If any errors occur during validation or authentication, the exceptions are caught and the errors are
        /// displayed to the user through the ErrorMessage parameter and setting the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
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
                    await AttemptLoginAsync(Username.Value, Password.Value);
                }
            }
            catch (ApiException e)
            {
                IsError = true;
                ErrorMessage = e.StatusCode == HttpStatusCode.Unauthorized || e.StatusCode == HttpStatusCode.Forbidden
                    ? Messages.ErrorMessageIncorrectCredentials
                    : Messages.ErrorMessageApiError;
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
        /// Private method that is invoked within the <see cref="LoginAsync"/> method. Its purpose
        /// is to call the <see cref="IAuthService"/> to retrieve the token and user information
        /// before navigating to either the verification page or home page, depending on the verification
        /// state of the user.
        /// </summary>
        /// <param name="username">The username to attempt authentication with.</param>
        /// <param name="password">The password to attempt authentication with/</param>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AttemptLoginAsync(string username, string password)
        {
            var token = await _authService.GetTokenAsync(new UserCredentials
            {
                Username = username,
                Password = password
            });

            var userResponse = await _authService.GetFromUsernameAsync(username, token);

            // Store the needed credentials in the store.
            await _storageService.SetAuthTokenAsync(token);
            await _storageService.SetUserIdAsync(userResponse.Id);
            await _storageService.SetUsernameAsync(username);
            await _storageService.SetPasswordAsync(password);

            // Need to ensure the correct details are registered for push notifications.
            await _restService.PostAsync("api/notification-management/v1/notifications/register",
                new NotificationRegistrationRequest
                {
                    UserId = await _storageService.GetUserIdAsync(),
                    DeviceGuid = (await _storageService.GetDeviceIdAsync()).ToString(),
                    Token = await _storageService.GetNotificationTokenAsync()
                }, token);
            
            if (!userResponse.Verified)
            {
                await NavigationService.NavigateAsync("VerificationPage");
            }
            else
            {
                await NavigationService.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage");
            }
        }

        /// <summary>
        /// Invoked when the <see cref="ForgottenPasswordCommand"/> is invoked by the view. All the method
        /// will do is navigate to the forgotten password page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ForgottenPasswordAsync()
        {
            await NavigationService.NavigateAsync("ForgottenPasswordPage");
        }
        
        /// <summary>
        /// Invoked when the user <see cref="RegisterCommand"/> is invoked by the view. All the method will do is
        /// navigate to the register page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task RegisterAsync()
        {
            await NavigationService.NavigateAsync("RegisterPage");
        }
    }
}