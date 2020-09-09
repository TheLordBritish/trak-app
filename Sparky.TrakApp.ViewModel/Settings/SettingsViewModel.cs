using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Prism.Navigation;
using ReactiveUI;
using Sparky.TrakApp.Common;
using Sparky.TrakApp.ViewModel.Common;

namespace Sparky.TrakApp.ViewModel.Settings
{
    /// <summary>
    /// The <see cref="SettingsViewModel"/> is a simple view model whose sole responsibility is to
    /// response to tap events on the settings page and redirect the user to the requested page.
    /// </summary>
    public class SettingsViewModel : ReactiveViewModel
    {
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        public SettingsViewModel(IScheduler scheduler, INavigationService navigationService) : base(scheduler, navigationService)
        {
            ChangePasswordCommand = ReactiveCommand.CreateFromTask(ChangePasswordAsync, outputScheduler: scheduler);
            ChangeEmailAddressCommand =
                ReactiveCommand.CreateFromTask(ChangeEmailAddressAsync, outputScheduler: scheduler);
        }
        
        /// <summary>
        /// Command that is invoked each the time the change password label is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="ChangePasswordAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; }

        /// <summary>
        /// Command that is invoked each the time the change email address label is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="ChangeEmailAddressAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangeEmailAddressCommand { get; }
        
        /// <summary>
        /// Private method that is invoked by the <see cref="ChangePasswordCommand"/> when activated by the associated
        /// view. This method will navigate the user to the change password page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ChangePasswordAsync()
        {
            await NavigationService.NavigateAsync("RequestChangePasswordPage", new NavigationParameters
            {
                { "transition-type", TransitionType.SlideFromRight }
            });
        }
        
        /// <summary>
        /// Private method that is invoked by the <see cref="ChangeEmailAddressCommand"/> when activated by the associated
        /// view. This method will navigate the user to the change email address page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ChangeEmailAddressAsync()
        {
            await NavigationService.NavigateAsync("ChangeEmailAddressPage", new NavigationParameters
            {
                { "transition-type", TransitionType.SlideFromRight }
            });
        }
    }
}