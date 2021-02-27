using System;
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
using SparkyStudios.TrakLibrary.Model.Settings;
using SparkyStudios.TrakLibrary.Model.Settings.Validation;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Settings
{
    /// <summary>
    /// The <see cref="ChangeEmailAddressViewModel"/> is a view model that is associated with the change email address page view.
    /// Its responsibility is to respond to change email address attempts made with credential information.
    ///
    /// The <see cref="ChangeEmailAddressViewModel"/> also provides methods to validate fields on the change email address page view.
    /// Any validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class ChangeEmailAddressViewModel : ReactiveViewModel, IValidate<ChangeEmailAddressDetails>
    {
        private readonly IStorageService _storageService;
        private readonly IAuthService _authService;
        private readonly IRestService _restService;
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
        public ChangeEmailAddressViewModel(IScheduler scheduler, INavigationService navigationService, IStorageService storageService, IAuthService authService, IRestService restService, IUserDialogs userDialogs) : base(scheduler, navigationService)
        {
            SetupForValidation();

            _storageService = storageService;
            _authService = authService;
            _restService = restService;
            _userDialogs = userDialogs;
            
            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation);

            ChangeEmailAddressCommand = ReactiveCommand.CreateFromTask(ChangeEmailAddressAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            ChangeEmailAddressCommand.ThrownExceptions.Subscribe(ex =>
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
                        Crashes.TrackError(ex);
                        break;
                }
            });
            
            this.WhenAnyObservable(x => x.ChangeEmailAddressCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated new email address with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> EmailAddress { get; private set; }
        
        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="ChangeEmailAddressViewModel"/> is the <see cref="EmailAddress"/> property.
        /// When the view is changed, the name is passed through and the request propagated to the
        /// <see cref="ClearValidation"/> methods.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }
        
        /// <summary>
        /// Command that is invoked by the view when the send button is tapped. When called, the command
        /// will propagate the request and call the <see cref="ChangeEmailAddressAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangeEmailAddressCommand { get; }
        
        /// <summary>
        /// Invoked within the constructor of the <see cref="ChangeEmailAddressViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="ChangeEmailAddressDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            EmailAddress = new Validatable<string>(nameof(ChangeEmailAddressDetails.EmailAddress));
            
            _validator = new ChangeEmailAddressDetailsValidator();
            _validatables = new Validatables(EmailAddress);
        }

        /// <summary>
        /// Validates the specified <see cref="ChangeEmailAddressDetails"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="ChangeEmailAddressDetailsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="ChangeEmailAddressDetails"/> instance to validate against the <see cref="ChangeEmailAddressDetailsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(ChangeEmailAddressDetails model)
        {
            return _validator.Validate(new ValidationContext<ChangeEmailAddressDetails>(model))
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="ChangeEmailAddressViewModel"/>.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="ChangeEmailAddressCommand"/> when activated by the associated
        /// view. This method will validate the new email address on the view, and if valid attempt to change the email address
        /// for currently authenticated user. If the request is successful, the user is logged out and presented with an alert
        /// message on the login page stating they'll need to login with their current credentials and re-verify.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ChangeEmailAddressAsync()
        {
            IsError = false;

            var changeEmailAddressDetails = _validatables.Populate<ChangeEmailAddressDetails>();
            var validationResult = Validate(changeEmailAddressDetails);

            if (validationResult.IsValidOverall)
            {
                await ExecuteChangeEmailAddressAsync();
            }
        }

        /// <summary>
        /// Private method invoked by the <see cref="ChangeEmailAddressAsync"/> method. When called it will call off to the API
        /// and attempt to change the users email address. If the email address changing was successful, the data within the storage
        /// service will be removed and the user will be re-directed back to the login page, prompting them to login with
        /// their current credentials and re-verify.
        ///
        /// If the call to the API was unsuccessful, an error message will be prompted to the user telling them to try
        /// again.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ExecuteChangeEmailAddressAsync()
        {
            // Get details from the storage service.
            var username = await _storageService.GetUsernameAsync();

            // Send the request to try and change the password.
            var response = await _authService.ChangeEmailAddressAsync(username, new ChangeEmailAddressRequest
            {
                EmailAddress = EmailAddress.Value
            });
            
            // If there are errors, it's either due to the email address being the same as the one provided.
            if (response.Error)
            {
                IsError = true;
                ErrorMessage = response.ErrorMessage;
            }
            else
            {
                // Email address changing was successful, continue with standard log out procedure. 
                var userId = await _storageService.GetUserIdAsync();
                var deviceId = await _storageService.GetDeviceIdAsync();

                // Need to ensure the correct details are registered for push notifications.
                await _restService.DeleteAsync(
                    $"notifications/unregister?user-id={userId}&device-guid={deviceId}");

                // Remove all of the identifiable information from the secure store.
                await _storageService.ClearCredentialsAsync();

                // Navigate back to the login page.
                await NavigationService.NavigateAsync("/LoginPage");

                var alertConfig = new AlertConfig()
                    .SetTitle(Messages.TrakTitle)
                    .SetMessage(Messages.ChangeEmailAddressPageSuccess);

                await _userDialogs.AlertAsync(alertConfig);
            }
        }
    }
}