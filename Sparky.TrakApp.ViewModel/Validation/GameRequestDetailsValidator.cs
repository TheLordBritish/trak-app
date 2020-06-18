using FluentValidation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
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