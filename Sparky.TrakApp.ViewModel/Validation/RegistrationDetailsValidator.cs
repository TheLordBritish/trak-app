using System.Linq;
using FluentValidation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    public class RegistrationDetailsValidator : AbstractValidator<RegistrationDetails>
    {
        public RegistrationDetailsValidator()
        {
            RuleFor(r => r.Username)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessageUsernameEmpty)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RegistrationErrorMessageUsernameWhitespace)
                .Length(1, 255)
                .WithMessage(Messages.RegistrationErrorMessageUsernameLength)
                .Matches(@"^\w+$")
                .WithMessage(Messages.RegistrationErrorMessageUsernameInvalidCharacters);

            RuleFor(r => r.EmailAddress)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessageEmailAddressEmpty)
                .EmailAddress()
                .WithMessage(Messages.RegistrationErrorMessageEmailAddressInvalid);

            RuleFor(r => r.Password)
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

            RuleFor(r => r.ConfirmPassword)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessageConfirmPasswordEmpty)
                .Must((model, field) => string.Equals(model.Password, field))
                .WithMessage(Messages.RegistrationErrorMessageConfirmPasswordMismatch);
        }
    }
}