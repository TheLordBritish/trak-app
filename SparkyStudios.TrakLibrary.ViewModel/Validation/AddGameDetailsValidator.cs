using FluentValidation;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Games.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="AddGameDetailsValidator"/> is a validation class that validates the properties
    /// within the <see cref="AddGameDetails"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
    public class AddGameDetailsValidator : AbstractValidator<AddGameDetails>
    {
        public AddGameDetailsValidator()
        {
            RuleFor(f => f.Platform)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .WithMessage(Messages.AddGameErrorMessagePlatformNull);

            RuleFor(f => f.Status)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEqual(GameUserEntryStatus.None)
                .WithMessage(Messages.AddGameErrorMessageStatusNone);
        }
    }
}