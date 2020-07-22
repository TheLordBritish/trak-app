using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentValidation;
using Plugin.FluentValidationRules;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Login.Validation;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Login
{
    /// <summary>
    /// The <see cref="ForgottenPasswordViewModel"/> is the view model that is associated with the forgotten password page view.
    /// Its responsibility is to send password reset emails with a randomly generated password.
    ///
    /// The <see cref="ForgottenPasswordViewModel"/> also provides methods to validate fields on the forgotten password page view. Any
    /// validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class ForgottenPasswordViewModel : BaseViewModel, IValidate<ForgottenPasswordDetails>
    {
        private readonly IAuthService _authService;

        private IValidator _validator;
        private Validatables _validatables;

        private Validatable<string> _emailAddress;
        
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="authService"> The <see cref="IAuthService"/> instance to inject.</param>
        public ForgottenPasswordViewModel(INavigationService navigationService, IAuthService authService) : base(navigationService)
        {
            _authService = authService;

            _emailAddress = new Validatable<string>(nameof(ForgottenPasswordDetails.EmailAddress));

            SetupForValidation();
        }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="VerificationViewModel"/> is the verification code. When the view is changed,
        /// the name is passed through and the request propagated to the <see cref="ClearValidation"/>
        /// methods.
        /// </summary>
        public ICommand ClearValidationCommand => new DelegateCommand<string>(ClearValidation);
        
        /// <summary>
        /// Command that is invoked by the view when the send button is tapped. When called, the command
        /// will propagate the request and call the <see cref="SendAsync"/> method.
        /// </summary>
        public ICommand SendCommand => new DelegateCommand(async () => await SendAsync());
        
        /// <summary>
        /// Command that is invoked by the view when the already have recovery token label is tapped. When
        /// called, the command will propagate the request and call the <see cref="RecoveryAsync"/> method.
        /// </summary>
        public ICommand RecoveryCommand => new DelegateCommand(async () => await RecoveryAsync());

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
        /// Invoked within the constructor of the <see cref="ForgottenPasswordViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="ForgottenPasswordDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
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
            return _validator.Validate(model)
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
        ///
        /// If any errors occur during validation or the email request, the exceptions are caught and the errors are
        /// displayed to the user through the ErrorMessage parameter and setting the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task SendAsync()
        {
            IsError = false;
            IsBusy = true;

            var registration = _validatables.Populate<ForgottenPasswordDetails>();
            var validationResult = Validate(registration);
            
            try
            {
                if (validationResult.IsValidOverall)
                {
                    await AttemptSendAsync(EmailAddress.Value);
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
        
        private async Task RecoveryAsync()
        {
            await NavigationService.NavigateAsync("RecoveryPage");
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="SendAsync"/> method. All this method
        /// will do is call off to the recover method on the <see cref="IAuthService"/> and navigate
        /// the user to the recovery page.
        /// </summary>
        /// <param name="emailAddress">The email address to send the password reset email to.</param>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AttemptSendAsync(string emailAddress)
        {
            await _authService.RequestRecoveryAsync(emailAddress);
            await NavigationService.NavigateAsync("RecoveryPage");
        }
    }
}