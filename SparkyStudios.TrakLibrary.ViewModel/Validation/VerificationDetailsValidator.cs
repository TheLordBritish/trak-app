using System.Linq;
using FluentValidation;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Login.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="VerificationDetailsValidator"/> is a validation class that validates the properties
    /// within the <see cref="VerificationDetails"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
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