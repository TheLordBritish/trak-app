using System.Linq;
using FluentValidation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Login.Validation;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    public class VerificationDetailsValidator : AbstractValidator<VerificationDetails>
    {
        public VerificationDetailsValidator()
        {
            RuleFor(v => v.VerificationCode)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.VerificationErrorMessageVerificationCodeEmpty)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.VerificationErrorMessageVerificationCodeWhitespace)
                .Length(1, 5)
                .WithMessage(Messages.VerificationErrorMessageVerificationCodeLength)
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage(Messages.VerificationErrorMessageVerificationCodeInvalidCharacters);
        }
    }
}