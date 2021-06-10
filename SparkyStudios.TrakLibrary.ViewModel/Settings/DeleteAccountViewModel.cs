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
using SparkyStudios.TrakLibrary.Model.Settings.Validation;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Settings
{
    /// <summary>
    /// The <see cref="DeleteAccountViewModel"/> is a view model that is associated with the delete account page view.
    /// Its responsibility is to respond to account deletion requests attempts made with valid inputs.
    ///
    /// The <see cref="DeleteAccountViewModel"/> also provides methods to validate fields on the delete account page view.
    /// Any validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class DeleteAccountViewModel : ReactiveViewModel, IValidate<DeleteAccountDetails>
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
        /// <param name="authService">The <see cref="IAuthService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        /// <param name="userDialogs">The <see cref="IUserDialogs"/> instance to inject.</param>
        public DeleteAccountViewModel(IScheduler scheduler, INavigationService navigationService, IStorageService storageService, IAuthService authService, IRestService restService, IUserDialogs userDialogs) : base(scheduler, navigationService)
        {
            SetupForValidation();

            _storageService = storageService;
            _authService = authService;
            _restService = restService;
            _userDialogs = userDialogs;
            
            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation, outputScheduler: scheduler);
            
            DeleteAccountCommand = ReactiveCommand.CreateFromTask(DeleteAccountAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            DeleteAccountCommand.ThrownExceptions.Subscribe(ex =>
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
            
            this.WhenAnyObservable(x => x.DeleteAccountCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading);
        }
        
        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated DELETE ME text with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> DeleteMe { get; private set; }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="DeleteAccountViewModel"/> is the <see cref="DeleteMe"/> property.
        /// When the view is changed, the name is passed through and the request propagated to the
        /// <see cref="ClearValidation"/> methods.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }
        
        /// <summary>
        /// Command that is invoked by the view when the Delete button is tapped. When called, the command
        /// will propagate the request and call the <see cref="DeleteAccountAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteAccountCommand { get; }
        
        /// <summary>
        /// Invoked within the constructor of the <see cref="DeleteAccountViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="DeleteAccountDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            DeleteMe = new Validatable<string>(nameof(DeleteAccountDetails.DeleteMe));
            
            _validator = new DeleteAccountDetailsValidator();
            _validatables = new Validatables(DeleteMe);
        }

        /// <summary>
        /// Validates the specified <see cref="DeleteAccountDetails"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="DeleteAccountDetailsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="DeleteAccountDetails"/> instance to validate against the <see cref="DeleteAccountDetailsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(DeleteAccountDetails model)
        {
            return _validator.Validate(new ValidationContext<DeleteAccountDetails>(model))
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
        /// Private method that is invoked by the <see cref="DeleteAccountCommand"/> when activated by the associated
        /// view. This method will validate the DELETE ME text on the view, and if valid attempt
        /// to delete the account of the currently authenticated user. If the request is successful, the user is logged out
        /// and presented with an alert message on the login page stating that their account has been deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task DeleteAccountAsync()
        {
            IsError = false;

            var deleteAccountDetails = _validatables.Populate<DeleteAccountDetails>();
            var validationResult = Validate(deleteAccountDetails);

            if (validationResult.IsValidOverall)
            {
                await ExecuteDeleteAccountAsync();
            }
        }

        /// <summary>
        /// Private method invoked by the <see cref="DeleteAccountAsync"/> method. When called it will call off to the API
        /// and attempt to delete the user. If the deletion was successful, the data within the storage
        /// service will be removed and the user will be re-directed back to the login page, telling them that their account
        /// has been successfully deleted.
        ///
        /// If the call to the API was unsuccessful, an error message will be prompted to the user telling them to try
        /// again.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ExecuteDeleteAccountAsync()
        {
            // Get details from the storage service.
            var userId = await _storageService.GetUserIdAsync();

            // Send the request to delete the account.
            await _authService.DeleteByIdAsync(userId);
            
            // Deletion was successful, continue with standard log out procedure. 
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
                .SetMessage(Messages.DeleteAccountPageSuccess);

            await _userDialogs.AlertAsync(alertConfig);
        }
    }
}