using FluentValidation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
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