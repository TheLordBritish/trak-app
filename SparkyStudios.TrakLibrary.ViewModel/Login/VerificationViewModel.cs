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
using SparkyStudios.TrakLibrary.Model.Login.Validation;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Validation;

namespace SparkyStudios.TrakLibrary.ViewModel.Login
{
    /// <summary>
    /// The <see cref="VerificationViewModel"/> is a simple view model that is associated with the verification page view.
    /// Its responsibility is to respond to verification attempts made with a verification code.
    ///
    /// The <see cref="VerificationViewModel"/> also provides methods to validate fields on the verification page view. Any
    /// validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class VerificationViewModel : ReactiveViewModel, IValidate<VerificationDetails>
    {
        private readonly IAuthService _authService;
        private readonly IStorageService _storageService;
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
        /// <param name="authService">The <see cref="IAuthService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="userDialogs">The <see cref="IUserDialogs"/> instance to inject.</param>
        public VerificationViewModel(IScheduler scheduler, INavigationService navigationService,
            IAuthService authService,
            IStorageService storageService, IUserDialogs userDialogs) : base(scheduler, navigationService)
        {
            _authService = authService;
            _storageService = storageService;
            _userDialogs = userDialogs;

            SetupForValidation();

            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation);

            VerifyCommand = ReactiveCommand.CreateFromTask(VerifyAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            VerifyCommand.ThrownExceptions.Subscribe(ex =>
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

            ResendVerificationCommand =
                ReactiveCommand.CreateFromTask(ResendVerificationAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            ResendVerificationCommand.ThrownExceptions.Subscribe(ex =>
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

            this.WhenAnyObservable(x => x.VerifyCommand.IsExecuting, x => x.ResendVerificationCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated verification code with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> VerificationCode { get; private set; }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="VerificationViewModel"/> is the verification code. When the view is changed,
        /// the name is passed through and the request propagated to the <see cref="ClearValidation"/>
        /// methods.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the verify button is tapped. When called, the command
        /// will propagate the request and call the <see cref="VerifyAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> VerifyCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the resend verification email label is tapped. When called,
        /// the command will propagate the request and calls the <see cref="ResendVerificationAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ResendVerificationCommand { get; }

        /// <summary>
        /// Invoked within the constructor of the <see cref="VerificationViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="VerificationDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            VerificationCode = new Validatable<string>(nameof(VerificationDetails.VerificationCode));

            _validator = new VerificationDetailsValidator();
            _validatables = new Validatables(VerificationCode);
        }

        /// <summary>
        /// Validates the specified <see cref="VerificationDetails"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="VerificationDetailsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="VerificationDetails"/> instance to validate against the <see cref="VerificationDetailsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(VerificationDetails model)
        {
            return _validator.Validate(new ValidationContext<VerificationDetails>(model))
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="VerificationViewModel"/>.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="VerifyCommand"/> when activated by the associated
        /// view. This method will validate the verification code supplied by the view, and if valid attempt to verify
        /// the user with the provided code. If verification is successful, the user will be navigated to the home page.
        ///
        /// If any errors occur during verification, the exceptions are caught and the errors are
        /// displayed to the user through the ErrorMessage parameter and setting the IsError boolean to true. The same
        /// is done if the calls were successful but the verification code was incorrect.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task VerifyAsync()
        {
            IsError = false;

            var registration = _validatables.Populate<VerificationDetails>();
            var validationResult = Validate(registration);

            if (validationResult.IsValidOverall)
            {
                await AttemptVerificationAsync(VerificationCode.Value);
            }
        }

        /// <summary>
        /// Private method that is invoked within the <see cref="VerifyAsync"/> method. Its purpose
        /// is to retrieve the needed credential data from the <see cref="IStorageService"/> and attempt
        /// to verify the user with the verification code supplied by the user via the view.
        ///
        /// If the user entered the correct verification code, then they will be navigated to the home page.
        /// However, if the user entered the incorrect verification code it is classed as an error an error
        /// message stating so is presented to the user.
        /// </summary>
        /// <param name="verificationCode">The verification to attempt verification with.</param>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AttemptVerificationAsync(string verificationCode)
        {
            var username = await _storageService.GetUsernameAsync();

            var response = await _authService.VerifyAsync(username, verificationCode);
            if (!response.Data)
            {
                IsError = response.Error;
                ErrorMessage = response.ErrorMessage;
            }
            else
            {
                await NavigationService.NavigateAsync("/BaseFlyoutPage/NavigationPage/HomePage");
            }
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="ResendVerificationCommand"/> when activated by the associated
        /// view. This method will call off to the server to resend an additional verification email to the logged in
        /// user, clearing all previous verification data. Once the email has been sent, the user is presented with a
        /// dialog box prompting them to check their emails.
        ///
        /// If any errors occur during resending, the exceptions are caught and the errors are displayed to the user through the
        /// ErrorMessage parameter and setting the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ResendVerificationAsync()
        {
            IsError = false;

            // Retrieve the needed credentials from the store.
            var username = await _storageService.GetUsernameAsync();

            // Send the re-verification request.
            await _authService.ReVerifyAsync(username);

            // On successful request, display a popup to the user stating that the email has been sent.
            await _userDialogs.AlertAsync(Messages.VerificationPageConfirmation, Messages.TrakTitle);
        }
    }
}