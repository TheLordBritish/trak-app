using FluentValidation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Games.Validation;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="GameRequestDetailsValidator"/> is a validation class that validates the properties
    /// within the <see cref="GameRequestDetails"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
    public class GameRequestDetailsValidator : AbstractValidator<GameRequestDetails>
    {
        public GameRequestDetailsValidator()
        {
            RuleFor(f => f.Title)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.GameRequestErrorMessageTitleEmpty)
                .MaximumLength(255)
                .WithMessage(Messages.GameRequestErrorMessageTitleMaxLength);

            RuleFor(f => f.Notes)
                .MaximumLength(2048)
                .WithMessage(Messages.GameRequestErrorMessageNotesMaxLength);
        }
    }
}