using System.Linq;
using FluentValidation;
using Sparky.TrakApp.Model.Settings.Validation;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="ChangePasswordDetailsValidator"/> is a validation class that validates the properties
    /// within the <see cref="ChangePasswordDetails"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
    public class ChangePasswordDetailsValidator : AbstractValidator<ChangePasswordDetails>
    {
        public ChangePasswordDetailsValidator()
        {
            RuleFor(r => r.RecoveryToken)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RecoveryErrorMessageRecoveryTokenEmpty)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RecoveryErrorMessageRecoveryTokenWhitespace)
                .Length(30)
                .WithMessage(Messages.RecoveryErrorMessageRecoveryTokenLength)
                .Matches("^[a-zA-Z][a-zA-Z0-9]*$")
                .WithMessage(Messages.RecoveryErrorMessageRecoveryTokenAlphanumeric);
            
            RuleFor(r => r.NewPassword)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessagePasswordEmpty)
                .MinimumLength(8)
                .WithMessage(Messages.RegistrationErrorMessagePasswordMinimumLength)
                .Matches("[A-Z]")
                .WithMessage(Messages.RegistrationErrorMessagePasswordUppercaseCharacter)
                .Matches("[a-z]")
                .WithMessage(Messages.RegistrationErrorMessagePasswordLowercaseCharacter)
                .Matches("[0-9]")
                .WithMessage(Messages.RegistrationErrorMessagePasswordNumber)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RegistrationErrorMessagePasswordWhitespace);

            RuleFor(r => r.ConfirmNewPassword)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessageConfirmPasswordEmpty)
                .Must((model, field) => string.Equals(model.NewPassword, field))
                .WithMessage(Messages.RegistrationErrorMessageConfirmPasswordMismatch);
        }
    }
}