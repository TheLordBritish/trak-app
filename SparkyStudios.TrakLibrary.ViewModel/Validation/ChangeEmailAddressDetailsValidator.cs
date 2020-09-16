using FluentValidation;
using SparkyStudios.TrakLibrary.Model.Settings.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Validation
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