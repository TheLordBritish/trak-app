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
    /// The <see cref="RecoveryViewModel"/> is a view model that is associated with the recovery page view.
    /// Its responsibility is to respond to recovery  attempts made with credential information.
    ///
    /// The <see cref="RecoveryViewModel"/> also provides methods to validate fields on the recovery page view. Any
    /// validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class RecoveryViewModel : BaseViewModel, IValidate<RecoveryDetails>
    {
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;

        private Validatable<string> _username;
        private Validatable<string> _recoveryToken;
        private Validatable<string> _password;
        private Validatable<string> _confirmPassword;

        private Validatables _validatables;
        private IValidator _validator;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService" /> instance to inject.</param>
        /// <param name="authService">The <see cref="IAuthService" /> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService" /> instance to inject.</param>
        public RecoveryViewModel(INavigationService navigationService, IAuthService authService,
            IStorageService storageService) : base(navigationService)
        {
            _authService = authService;
            _storageService = storageService;

            Username = new Validatable<string>(nameof(RecoveryDetails.Username));
            RecoveryToken = new Validatable<string>(nameof(RecoveryDetails.RecoveryToken));
            Password = new Validatable<string>(nameof(RecoveryDetails.Password));
            ConfirmPassword = new Validatable<string>(nameof(RecoveryDetails.ConfirmPassword));

            SetupForValidation();
        }

        /// <summary>
        /// Command that is invoked each time that a validatable field on the view is changed, which
        /// for the <see cref="RecoveryViewModel" /> is the username, recovery token, password and confirm
        /// password. When the view is changed, the name is passed through and the request propagated
        /// to the <see cref="ClearValidation" /> method.
        /// </summary>
        public ICommand ClearValidationCommand => new DelegateCommand<string>(ClearValidation);

        /// <summary>
        /// Command that is invoked by the view when the recover button is tapped. When called, the command
        /// will propagate the request and call the <see cref="RecoverAsync"/> method.
        /// </summary>
        public ICommand RecoverCommand => new DelegateCommand(async () => await RecoverAsync());

        /// <summary>
        /// Command that is invoked by the view when the login label is tapped. When called, the command
        /// will propagate the request and call the <see cref="LoginAsync"/> method.
        /// </summary>
        public ICommand LoginCommand => new DelegateCommand(async () => await LoginAsync());

        /// <summary>
        /// A <see cref="Validatable{T}" /> that contains the currently populated username with
        /// additional validation information.
        /// </summary>
        public Validatable<string> Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        /// <summary>
        /// A <see cref="Validatable{T}" /> that contains the currently populated recovery token with
        /// additional validation information.
        /// </summary>
        public Validatable<string> RecoveryToken
        {
            get => _recoveryToken;
            set => SetProperty(ref _recoveryToken, value);
        }

        /// <summary>
        /// A <see cref="Validatable{T}" /> that contains the currently populated password with
        /// additional validation information.
        /// </summary>
        public Validatable<string> Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        /// <summary>
        /// A <see cref="Validatable{T}" /> that contains the currently populated password confirmation with
        /// additional validation information.
        /// </summary>
        public Validatable<string> ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        /// <summary>
        /// Invoked within the constructor of the <see cref="RecoveryViewModel" />, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}" /> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="RecoveryDetailsValidator" /> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
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
            return _validator.Validate(model)
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
            IsBusy = true;

            var recovery = _validatables.Populate<RecoveryDetails>();
            var validationResult = Validate(recovery);

            try
            {
                if (validationResult.IsValidOverall)
                {
                    await AttemptRecoveryAsync(Username.Value, RecoveryToken.Value, Password.Value);
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
        private async Task AttemptRecoveryAsync(string username, string recoveryToken, string password)
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
                var token = await _authService.GetTokenAsync(new UserCredentials
                {
                    Username = username,
                    Password = password
                });

                // Store the needed credentials in the store.
                await _storageService.SetAuthTokenAsync(token);
                await _storageService.SetUserIdAsync(user.Id);
                await _storageService.SetUsernameAsync(user.Username);

                // Navigate to the verification page for the user to verify their account before use.
                await NavigationService.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage");
            }
        }
    }
}