using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AppCenter.Crashes;
using Plugin.FluentValidationRules;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Login.Validation;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Login
{
    /// <summary>
    /// The <see cref="RecoveryViewModel"/> is a view model that is associated with the recovery page view.
    /// Its responsibility is to respond to recovery  attempts made with credential information.
    ///
    /// The <see cref="RecoveryViewModel"/> also provides methods to validate fields on the recovery page view. Any
    /// validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class RecoveryViewModel : ReactiveViewModel, IValidate<RecoveryDetails>
    {
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;

        private Validatables _validatables;
        private IValidator _validator;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService" /> instance to inject.</param>
        /// <param name="authService">The <see cref="IAuthService" /> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService" /> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        public RecoveryViewModel(IScheduler scheduler, INavigationService navigationService, IAuthService authService,
            IStorageService storageService, IRestService restService) : base(scheduler, navigationService)
        {
            _authService = authService;
            _storageService = storageService;
            _restService = restService;

            SetupForValidation();

            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation);

            RecoverCommand = ReactiveCommand.CreateFromTask(RecoverAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            RecoverCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                switch (ex)
                {
                    case TaskCanceledException _:
                        ErrorMessage = Messages.ErrorMessageNoInternet;
                        break;
                    case ApiException _:
                        ErrorMessage = Messages.ErrorMessageApiError;
                        break;
                    default:
                        ErrorMessage = Messages.ErrorMessageGeneric;
                        Crashes.TrackError(ex, new Dictionary<string, string>
                        {
                            {"Username", Username.Value}
                        });
                        break;
                }
            });

            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, outputScheduler: scheduler);

            this.WhenAnyObservable(x => x.RecoverCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading);
        }

        /// <summary>
        /// A <see cref="Validatable{T}" /> that contains the currently populated username with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> Username { get; private set; }

        /// <summary>
        /// A <see cref="Validatable{T}" /> that contains the currently populated recovery token with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> RecoveryToken { get; private set; }

        /// <summary>
        /// A <see cref="Validatable{T}" /> that contains the currently populated password with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> Password { get; private set; }

        /// <summary>
        /// A <see cref="Validatable{T}" /> that contains the currently populated password confirmation with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> ConfirmPassword { get; private set; }

        /// <summary>
        /// Command that is invoked each time that a validatable field on the view is changed, which
        /// for the <see cref="RecoveryViewModel" /> is the username, recovery token, password and confirm
        /// password. When the view is changed, the name is passed through and the request propagated
        /// to the <see cref="ClearValidation" /> method.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the recover button is tapped. When called, the command
        /// will propagate the request and call the <see cref="RecoverAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RecoverCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the login label is tapped. When called, the command
        /// will propagate the request and call the <see cref="LoginAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        /// <summary>
        /// Invoked within the constructor of the <see cref="RecoveryViewModel" />, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}" /> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="RecoveryDetailsValidator" /> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            Username = new Validatable<string>(nameof(RecoveryDetails.Username));
            RecoveryToken = new Validatable<string>(nameof(RecoveryDetails.RecoveryToken));
            Password = new Validatable<string>(nameof(RecoveryDetails.Password));
            ConfirmPassword = new Validatable<string>(nameof(RecoveryDetails.ConfirmPassword));

            _validator = new RecoveryDetailsValidator();
            _validatables = new Validatables(Username, RecoveryToken, Password, ConfirmPassword);
        }

        /// <summary>
        /// Validates the specified <see cref="RecoveryDetails" /> model with the validation rules specified within
        /// this class, which are contained within the <see cref="RecoveryDetailsValidator" />. The results, regardless
        /// of whether they are true or false are applied to the validatable variable.
        /// </summary>
        /// <param name="model">
        /// The <see cref="RecoveryDetails" /> instance to validate against the <see cref="RecoveryDetailsValidator" />.
        /// </param>
        /// <returns>A <see cref="OverallValidationResult" /> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(RecoveryDetails model)
        {
            return _validator.Validate(new ValidationContext<RecoveryDetails>(model))
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="RecoveryViewModel" />.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="RecoverCommand"/> when activated by the associated
        /// view. This method will validate the different member variables within this view model, the username, recovery
        /// token and password supplied by the view, and if valid attempt to recover the account with the provided
        /// information. If recovery is successful, the user will be navigated to the home page.
        ///
        /// If any errors occur during recovery, the exceptions are caught and the errors are
        /// displayed to the user through the ErrorMessage parameter and setting the IsError boolean to true. The same
        /// is done if the calls were successful but recovery failed for other reasons, such as the username being
        /// incorrect or the account not being in the recovery position.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task RecoverAsync()
        {
            IsError = false;

            var recovery = _validatables.Populate<RecoveryDetails>();
            var validationResult = Validate(recovery);

            if (validationResult.IsValidOverall)
            {
                await AttemptRecoverAsync(Username.Value, RecoveryToken.Value, Password.Value);
            }
        }

        /// <summary>
        /// Invoked when the user <see cref="LoginCommand"/> is invoked by the view. All the method will do is
        /// navigate back to the login page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoginAsync()
        {
            await NavigationService.NavigateAsync("LoginPage");
        }

        /// <summary>
        /// Private method that is invoked within the <see cref="RecoverAsync"/> method. Its purpose
        /// is to attempt to recover an existing user with the supplied information by calling off to an external
        /// service, before storing some small amount of information for later use. 
        ///
        /// If the user entered valid recovery information, then they will be navigated to the home page. However,
        /// if the user entered information that is invalid, then an error message stating that it is already in use will
        /// be presented to the user.
        /// </summary>
        /// <param name="username">The username to attempt recovery with.</param>
        /// <param name="recoveryToken">The recovery token to attempt recovery with.</param>
        /// <param name="password">The password to attempt recovery with.</param>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AttemptRecoverAsync(string username, string recoveryToken, string password)
        {
            // Attempt to recover an existing account.
            var userRecoveryResponse = await _authService.RecoverAsync(new RecoveryRequest
            {
                Username = username,
                RecoveryToken = recoveryToken,
                Password = password
            });

            // If there are errors with the registration, display them to the user.
            if (userRecoveryResponse.Error)
            {
                IsError = true;
                ErrorMessage = userRecoveryResponse.ErrorMessage;
            }
            else
            {
                // If there are no issues, retrieve the authenticated token.
                var user = userRecoveryResponse.Data;
                var token = await _authService.GetTokenAsync(new LoginRequest
                {
                    Username = username,
                    Password = password
                });

                // Store the needed credentials in the store.
                await _storageService.SetAuthTokenAsync(token);
                await _storageService.SetUserIdAsync(user.Id);
                await _storageService.SetUsernameAsync(user.Username);

                // Need to ensure the correct details are registered for push notifications.
                await _restService.PostAsync("notifications/register",
                    new NotificationRegistrationRequest
                    {
                        UserId = await _storageService.GetUserIdAsync(),
                        DeviceGuid = (await _storageService.GetDeviceIdAsync()).ToString(),
                        Token = await _storageService.GetNotificationTokenAsync()
                    });

                // Navigate to the verification page for the user to verify their account before use.
                await NavigationService.NavigateAsync("/BaseFlyoutPage/NavigationPage/HomePage");
            }
        }
    }
}