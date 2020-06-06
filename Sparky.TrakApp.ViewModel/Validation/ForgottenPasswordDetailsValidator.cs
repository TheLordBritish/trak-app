using System.Linq;
using FluentValidation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    public class ForgottenPasswordDetailsValidator : AbstractValidator<ForgottenPasswordDetails>
    {
        public ForgottenPasswordDetailsValidator()
        {
            RuleFor(f => f.EmailAddress)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.ForgottenPasswordErrorMessageEmailAddressEmpty)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.VerificationErrorMessageVerificationCodeWhitespace)
                .EmailAddress()
                .WithMessage(Messages.ForgottenPasswordErrorMessageEmailAddressInvalid);
        }
    }
}