using System.Linq;
using FluentValidation;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Login.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="ForgottenPasswordDetailsValidator"/> is a validation class that validates the properties
    /// within the <see cref="ForgottenPasswordDetails"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
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