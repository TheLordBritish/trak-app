using FluentValidation;
using Sparky.TrakApp.Model.Settings.Validation;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    public class ChangeEmailAddressDetailsValidator : AbstractValidator<ChangeEmailAddressDetails>
    {
        public ChangeEmailAddressDetailsValidator()
        {
            RuleFor(r => r.EmailAddress)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessageEmailAddressEmpty)
                .EmailAddress()
                .WithMessage(Messages.RegistrationErrorMessageEmailAddressInvalid);
        }
    }
}