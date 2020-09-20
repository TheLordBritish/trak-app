using FluentValidation;
using SparkyStudios.TrakLibrary.Model.Settings.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="DeleteAccountDetailsValidator"/> is a validation class that validates the properties
    /// within the <see cref="DeleteAccountDetails"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
    public class DeleteAccountDetailsValidator : AbstractValidator<DeleteAccountDetails>
    {
        public DeleteAccountDetailsValidator()
        {
            RuleFor(f => f.DeleteMe)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.DeleteAccountIncorrectDeleteMessage)
                .Must(p => p.Equals(Messages.DeleteAccountPageDeleteMe))
                .WithMessage(Messages.DeleteAccountIncorrectDeleteMessage);
        }
    }
}