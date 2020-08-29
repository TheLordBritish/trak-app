using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Acr.UserDialogs;
using FluentValidation;
using Microsoft.AppCenter.Crashes;
using Plugin.FluentValidationRules;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sparky.TrakApp.Model.Settings;
using Sparky.TrakApp.Model.Settings.Validation;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Common;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Settings
{
    /// <summary>
    /// The <see cref="ChangePasswordViewModel"/> is a view model that is associated with the change password page view.
    /// Its responsibility is to respond to change password attempts made with credential information.
    ///
    /// The <see cref="ChangePasswordViewModel"/> also provides methods to validate fields on the change password page view.
    /// Any validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class ChangePasswordViewModel : ReactiveViewModel, IValidate<ChangePasswordDetails>
    {
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;
        private readonly IAuthService _authService;
        private readonly IUserDialogs _userDialogs;

        private IValidator _validator;
        private Validatables _validatables;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        /// <param name="authService">The <see cref="IAuthService"/> instance to inject.</param>
        /// <param name="userDialogs">The <see cref="IUserDialogs"/> instance to inject.</param>
        public ChangePasswordViewModel(IScheduler scheduler, INavigationService navigationService,
            IStorageService storageService, IRestService restService, IAuthService authService,
            IUserDialogs userDialogs) : base(scheduler, navigationService)
        {
            _storageService = storageService;
            _restService = restService;
            _authService = authService;
            _userDialogs = userDialogs;

            SetupForValidation();

            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation);

            ChangeCommand = ReactiveCommand.CreateFromTask(ChangeAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            ChangeCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;

                if (ex is ApiException)
                {
                    ErrorMessage = Messages.ErrorMessageApiError;
                }
                else
                {
                    ErrorMessage = Messages.ErrorMessageGeneric;
                    Crashes.TrackError(ex);
                }
            });
            
            this.WhenAnyObservable(x => x.ChangeCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated reset token with
        /// additional validation information.
        /// </summary>
        public Validatable<string> ResetToken { get; private set; }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated new password with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> NewPassword { get; private set; }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated confirm new password with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> ConfirmNewPassword { get; private set; }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="ChangePasswordViewModel"/> is the <see cref="NewPassword"/> and <see cref="ConfirmNewPassword"/>.
        /// When the view is changed, the name is passed through and the request propagated to the
        /// <see cref="ClearValidation"/> methods.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the send button is tapped. When called, the command
        /// will propagate the request and call the <see cref="ChangeAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangeCommand { get; }

        /// <summary>
        /// Invoked within the constructor of the <see cref="ChangePasswordViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="ChangePasswordDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            ResetToken = new Validatable<string>(nameof(ChangePasswordDetails.RecoveryToken));
            NewPassword = new Validatable<string>(nameof(ChangePasswordDetails.NewPassword));
            ConfirmNewPassword = new Validatable<string>(nameof(ChangePasswordDetails.ConfirmNewPassword));

            _validator = new ChangePasswordDetailsValidator();
            _validatables = new Validatables(ResetToken, NewPassword, ConfirmNewPassword);
        }

        /// <summary>
        /// Validates the specified <see cref="ChangePasswordDetails"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="ForgottenPasswordDetailsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="ChangePasswordDetails"/> instance to validate against the <see cref="ChangePasswordDetailsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(ChangePasswordDetails model)
        {
            return _validator.Validate(model)
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="ChangePasswordViewModel"/>.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="ChangeCommand"/> when activated by the associated
        /// view. This method will validate the new password and its confirmation on the view, and if valid attempt
        /// to change the password for currently authenticated user. If the request is successful, the user is logged out
        /// and presented with an alert message on the login page stating they'll need to login with their new credentials.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ChangeAsync()
        {
            IsError = false;

            var changePasswordDetails = _validatables.Populate<ChangePasswordDetails>();
            var validationResult = Validate(changePasswordDetails);

            if (validationResult.IsValidOverall)
            {
                await ExecuteChangeAsync();
            }
        }

        /// <summary>
        /// Private method invoked by the <see cref="ChangeAsync"/> method. When called it will call off to the API
        /// and attempt to change the users password. If the password changing was successful, the data within the storage
        /// service will be removed and the user will be re-directed back to the login page, prompting them to login with
        /// their new credentials.
        ///
        /// If the call to the API was unsuccessful, an error message will be prompted to the user telling them to try
        /// again.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ExecuteChangeAsync()
        {
            // Get details from the storage service.
            var username = await _storageService.GetUsernameAsync();

            // Send the request to try and change the password.
            var response = await _authService.ChangePasswordAsync(new ChangePasswordRequest
            {
                Username = username,
                NewPassword = NewPassword.Value,
                RecoveryToken = ResetToken.Value
            });

            // If there are errors, it's either due to the user not being found, the password not matching the criteria 
            // or the recovery token entered was incorrect.
            if (response.Error)
            {
                IsError = true;
                ErrorMessage = response.ErrorMessage;
            }
            else
            {
                // Password changing was successful, continue with standard log out procedure. 
                var userId = await _storageService.GetUserIdAsync();
                var deviceId = await _storageService.GetDeviceIdAsync();

                // Need to ensure the correct details are registered for push notifications.
                await _restService.DeleteAsync(
                    $"notifications/unregister?user-id={userId}&device-guid={deviceId}");

                // Remove all of the identifiable information from the secure store.
                await _storageService.SetUsernameAsync(string.Empty);
                await _storageService.SetAuthTokenAsync(string.Empty);
                await _storageService.SetUserIdAsync(0);

                // Navigate back to the login page.
                await NavigationService.NavigateAsync("/LoginPage");

                var alertConfig = new AlertConfig()
                    .SetTitle(Messages.TrakTitle)
                    .SetMessage(Messages.ChangePasswordPageSuccess);

                await _userDialogs.AlertAsync(alertConfig);
            }
        }
    }
}