using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.ViewModel.Common;

namespace SparkyStudios.TrakLibrary.ViewModel.Settings
{
    /// <summary>
    /// The <see cref="SettingsViewModel"/> is a simple view model whose sole responsibility is to
    /// response to tap events on the settings page and redirect the user to the requested page.
    /// </summary>
    public class SettingsViewModel : ReactiveViewModel
    {
        private readonly IStorageService _storageService;
        
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        public SettingsViewModel(IScheduler scheduler, INavigationService navigationService, IStorageService storageService) : base(scheduler, navigationService)
        {
            _storageService = storageService;

            ChangeUsernameCommand = ReactiveCommand.CreateFromTask(ChangeUsernameAsync, outputScheduler: scheduler);
            ChangePasswordCommand = ReactiveCommand.CreateFromTask(ChangePasswordAsync, outputScheduler: scheduler);
            ChangeEmailAddressCommand =
                ReactiveCommand.CreateFromTask(ChangeEmailAddressAsync, outputScheduler: scheduler);
            DeleteAccountCommand = ReactiveCommand.CreateFromTask(DeleteAccountAsync, outputScheduler: scheduler);
        }
        
        /// <summary>
        /// The current username of the logged in user. Used for a personalisation message at displayed to the user.
        /// </summary>
        [Reactive]
        public string Username { get; set; }
        
        /// <summary>
        /// Command that is invoked each the time the change username label is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="ChangeUsernameAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangeUsernameCommand { get; }
        
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
        /// Command that is invoked each the time the change delete my account label is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="DeleteAccountAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteAccountCommand { get; }

        /// <summary>
        /// Overriden method that is automatically invoked when the page is navigated to. Its purpose is to set the username
        /// of the currently logged in user.
        /// </summary>
        /// <param name="parameters">The <see cref="NavigationParameters" />, which contains information for display purposes.</param>
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            
            // Set the username here to avoid async calls in constructor.
            Username = await _storageService.GetUsernameAsync();
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="ChangeUsernameCommand"/> when activated by the associated
        /// view. This method will navigate the user to the change username page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ChangeUsernameAsync()
        {
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Private method that is invoked by the <see cref="ChangePasswordCommand"/> when activated by the associated
        /// view. This method will navigate the user to the change password page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ChangePasswordAsync()
        {
            await NavigationService.NavigateAsync("RequestChangePasswordPage");
        }
        
        /// <summary>
        /// Private method that is invoked by the <see cref="ChangeEmailAddressCommand"/> when activated by the associated
        /// view. This method will navigate the user to the change email address page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ChangeEmailAddressAsync()
        {
            await NavigationService.NavigateAsync("ChangeEmailAddressPage");
        }
        
        /// <summary>
        /// Private method that is invoked by the <see cref="DeleteAccountCommand"/> when activated by the associated
        /// view. This method will navigate the user to the delete account page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task DeleteAccountAsync()
        {
            await NavigationService.NavigateAsync("DeleteAccountPage");
        }
    }
}