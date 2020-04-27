using FluentValidation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    public class UserCredentialsValidator : AbstractValidator<UserCredentials>
    {
        public UserCredentialsValidator()
        {
            RuleFor(r => r.Username)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.LoginErrorMessageUsernameEmpty);

            RuleFor(r => r.Password)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.LoginErrorMessagePasswordEmpty);
        }
    }
}