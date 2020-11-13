using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
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
    /// The <see cref="ForgottenPasswordViewModel"/> is the view model that is associated with the forgotten password page view.
    /// Its responsibility is to send password reset emails with a randomly generated password.
    ///
    /// The <see cref="ForgottenPasswordViewModel"/> also provides methods to validate fields on the forgotten password page view. Any
    /// validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class ForgottenPasswordViewModel : ReactiveViewModel, IValidate<ForgottenPasswordDetails>
    {
        private readonly IAuthService _authService;

        private IValidator _validator;
        private Validatables _validatables;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="authService"> The <see cref="IAuthService"/> instance to inject.</param>
        public ForgottenPasswordViewModel(IScheduler scheduler, INavigationService navigationService,
            IAuthService authService) : base(scheduler, navigationService)
        {
            _authService = authService;

            SetupForValidation();

            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation);

            SendCommand = ReactiveCommand.CreateFromTask(SendAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            SendCommand.ThrownExceptions.Subscribe(ex =>
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

            RecoveryCommand = ReactiveCommand.CreateFromTask(RecoveryAsync, outputScheduler: scheduler);

            this.WhenAnyObservable(x => x.SendCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated email address with
        /// additional validation information.
        /// </summary>
        [Reactive]
        public Validatable<string> EmailAddress { get; private set; }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="VerificationViewModel"/> is the verification code. When the view is changed,
        /// the name is passed through and the request propagated to the <see cref="ClearValidation"/>
        /// methods.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the send button is tapped. When called, the command
        /// will propagate the request and call the <see cref="SendAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SendCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the already have recovery token label is tapped. When
        /// called, the command will propagate the request and call the <see cref="RecoveryAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RecoveryCommand { get; }

        /// <summary>
        /// Invoked within the constructor of the <see cref="ForgottenPasswordViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="ForgottenPasswordDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            EmailAddress = new Validatable<string>(nameof(ForgottenPasswordDetails.EmailAddress));

            _validator = new ForgottenPasswordDetailsValidator();
            _validatables = new Validatables(EmailAddress);
        }

        /// <summary>
        /// Validates the specified <see cref="ForgottenPasswordDetails"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="ForgottenPasswordDetailsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="ForgottenPasswordDetails"/> instance to validate against the <see cref="ForgottenPasswordDetailsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(ForgottenPasswordDetails model)
        {
            return _validator.Validate(new ValidationContext<ForgottenPasswordDetails>(model))
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="ForgottenPasswordViewModel"/>.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="SendCommand"/> when activated by the associated
        /// view. This method will validate the email address on the view, and if valid attempt to send a password
        /// reset email to the specified address. If the request is successful, the user is navigated to the recovery
        /// page with further details.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task SendAsync()
        {
            IsError = false;

            var registration = _validatables.Populate<ForgottenPasswordDetails>();
            var validationResult = Validate(registration);

            if (validationResult.IsValidOverall)
            {
                await _authService.RequestRecoveryAsync(EmailAddress.Value);
                await NavigationService.NavigateAsync("RecoveryPage");
            }
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="RecoveryCommand"/> when activated by the associated view.
        /// This method will merely push the recovery page to the top of the view stack.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task RecoveryAsync()
        {
            await NavigationService.NavigateAsync("RecoveryPage");
        }
    }
}