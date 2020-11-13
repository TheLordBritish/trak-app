using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using Prism.Navigation;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AppCenter.Crashes;
using Microsoft.IdentityModel.Tokens;
using Plugin.FluentValidationRules;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Login
{
    /// <summary>
    /// The <see cref="LoginViewModel"/> is a simple view model that is associated with the login page view.
    /// Its responsibility is to respond to login attempts made with a username and password and to allow the user
    /// a way to navigate to the registration page.
    ///
    /// The <see cref="LoginViewModel"/> also provides methods to validate fields on the login page view. Any
    /// validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class LoginViewModel : ReactiveViewModel, IValidate<UserCredentials>
    {
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;
        private readonly SecurityTokenHandler _securityTokenHandler;
        
        private IValidator _validator;
        private Validatables _validatables;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="authService">The <see cref="IAuthService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        /// <param name="securityTokenHandler">The <see cref="SecurityTokenHandler"/> instance to inject.</param>
        public LoginViewModel(IScheduler scheduler, INavigationService navigationService, IAuthService authService,
            IStorageService storageService, IRestService restService, SecurityTokenHandler securityTokenHandler)
            : base(scheduler, navigationService)
        {
            _authService = authService;
            _storageService = storageService;
            _restService = restService;
            _securityTokenHandler = securityTokenHandler;

            SetupForValidation();

            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation);

            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            LoginCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;

                if (ex is ApiException e)
                {
                    ErrorMessage = e.StatusCode == HttpStatusCode.Unauthorized ||
                                   e.StatusCode == HttpStatusCode.Forbidden
                        ? Messages.ErrorMessageIncorrectCredentials
                        : Messages.ErrorMessageApiError;
                }
                else
                {
                    ErrorMessage = Messages.ErrorMessageGeneric;
                    Crashes.TrackError(ex, new Dictionary<string, string>
                    {
                        {"Username", Username.Value}
                    });
                }
            });

            ForgottenPasswordCommand =
                ReactiveCommand.CreateFromTask(ForgottenPasswordAsync, outputScheduler: scheduler);

            RegisterCommand = ReactiveCommand.CreateFromTask(RegisterAsync, outputScheduler: scheduler);

            this.WhenAnyObservable(x => x.LoginCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated username with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> Username { get; private set; }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated password with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> Password { get; private set; }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="LoginViewModel"/> is the username and password. When the view is changed,
        /// the name is passed through and the request propagated to the <see cref="ClearValidation"/>
        /// methods.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the login button is tapped. When called, the command
        /// will propagate the request and call the <see cref="LoginAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the forgotten password label is tapped. When called,
        /// the command will propagate the request and call the <see cref="ForgottenPasswordAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ForgottenPasswordCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the register label is tapped. When called, the command
        /// will propagate the request and call the <see cref="RegisterAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }

        /// <summary>
        /// Invoked within the constructor of the <see cref="LoginViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="UserCredentialsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            Username = new Validatable<string>(nameof(UserCredentials.Username));
            Password = new Validatable<string>(nameof(UserCredentials.Password));

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
            return _validator.Validate(new ValidationContext<UserCredentials>(model))
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

            var registration = _validatables.Populate<UserCredentials>();
            var validationResult = Validate(registration);

            if (validationResult.IsValidOverall)
            {
                var token = await _authService.GetTokenAsync(new UserCredentials
                {
                    Username = Username.Value,
                    Password = Password.Value
                });
                
                // decode the jwt.
                var jwt = _securityTokenHandler.ReadToken(token) as JwtSecurityToken;
                
                // Get the needed information from the JWT.
                var username = jwt.Subject;
                var userId = int.Parse(jwt.Claims.First(c => c.Type == "userId").Value);
                var verified = bool.Parse(jwt.Claims.First(c => c.Type == "verified").Value);
                
                // Store the needed credentials in the store.
                await _storageService.SetAuthTokenAsync(token);
                await _storageService.SetUserIdAsync(userId);
                await _storageService.SetUsernameAsync(username);

                // Need to ensure the correct details are registered for push notifications.
                await _restService.PostAsync("notifications/register",
                    new NotificationRegistrationRequest
                    {
                        UserId = await _storageService.GetUserIdAsync(),
                        DeviceGuid = (await _storageService.GetDeviceIdAsync()).ToString(),
                        Token = await _storageService.GetNotificationTokenAsync()
                    });

                if (!verified)
                {
                    await NavigationService.NavigateAsync("VerificationPage");
                }
                else
                {
                    await NavigationService.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage");
                }
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