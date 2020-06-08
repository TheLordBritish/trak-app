using System.Linq;
using FluentValidation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    public class RecoveryDetailsValidator : AbstractValidator<RecoveryDetails>
    {
        public RecoveryDetailsValidator()
        {
            RuleFor(r => r.Username)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RecoveryErrorMessageUsernameEmpty)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RecoveryErrorMessageUsernameWhitespace)
                .Length(1, 255)
                .WithMessage(Messages.RecoveryErrorMessageUsernameLength)
                .Matches(@"^\w+$")
                .WithMessage(Messages.RecoveryErrorMessageUsernameInvalidCharacters);

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
            
            RuleFor(r => r.Password)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RecoveryErrorMessagePasswordEmpty)
                .MinimumLength(8)
                .WithMessage(Messages.RecoveryErrorMessagePasswordMinimumLength)
                .Matches("[A-Z]")
                .WithMessage(Messages.RecoveryErrorMessagePasswordUppercaseCharacter)
                .Matches("[a-z]")
                .WithMessage(Messages.RecoveryErrorMessagePasswordLowercaseCharacter)
                .Matches("[0-9]")
                .WithMessage(Messages.RecoveryErrorMessagePasswordNumber)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RecoveryErrorMessagePasswordWhitespace);

            RuleFor(r => r.ConfirmPassword)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RecoveryErrorMessageConfirmPasswordEmpty)
                .Must((model, field) => string.Equals(model.Password, field))
                .WithMessage(Messages.RecoveryErrorMessageConfirmPasswordMismatch);
        }
    }
}