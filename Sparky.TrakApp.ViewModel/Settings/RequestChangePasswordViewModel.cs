using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sparky.TrakApp.Common;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Common;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Settings
{
    /// <summary>
    /// The <see cref="RequestChangePasswordViewModel"/> is a simple view model that responds to event
    /// propagated by the request change password page. It's mainly contains one element of functionality,
    /// which is to dispatch change password emails to the currently logged in users' email address, which
    /// allows the user to actually reset their password.
    /// </summary>
    public class RequestChangePasswordViewModel : ReactiveViewModel
    {
        private readonly IStorageService _storageService;
        private readonly IAuthService _authService;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="authService">The <see cref="IAuthService"/> instance to inject.</param>
        public RequestChangePasswordViewModel(IScheduler scheduler, INavigationService navigationService,
            IStorageService storageService, IAuthService authService) : base(scheduler, navigationService)
        {
            _storageService = storageService;
            _authService = authService;

            SendCommand = ReactiveCommand.CreateFromTask(SendAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            SendCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (ex is ApiException)
                {
                    ErrorMessage = Messages.ErrorMessageApiError;
                }
                else
                {
                    ErrorMessage = Messages.ErrorMessageGeneric;
                    Crashes.TrackError(ex);
                }
            });

            ChangePasswordCommand = ReactiveCommand.CreateFromTask(ChangePasswordAsync, outputScheduler: scheduler);

            this.WhenAnyObservable(x => x.SendCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading);
        }

        /// <summary>
        /// Command that is invoked by the view when the send button is tapped. When called, the command
        /// will propagate the request and call the <see cref="SendAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SendCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the already got a token label is tapped. When called,
        /// the command will propagate the request and call the <see cref="ChangePasswordAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; }

        /// <summary>
        /// Invoked when the <see cref="SendCommand"/> is invoked by the view. The method will attempt
        /// to send an email to the email address registered against this user with a recovery token which
        /// will be needed when changing their password. If the call is successful, the user will be
        /// redirected to the change password page.
        ///
        /// If the call is unsuccessful, the user will not be re-directed and an error message will be
        /// presented to the user prompted them to dry again.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task SendAsync()
        {
            IsError = false;

            var username = await _storageService.GetUsernameAsync();

            await _authService.RequestChangePasswordAsync(username);
            await NavigationService.NavigateAsync("ChangePasswordPage", new NavigationParameters
            {
                { "transition-type", TransitionType.SlideFromBottom }
            });
        }

        /// <summary>
        /// Invoked when the <see cref="ChangePasswordCommand"/> is invoked by the view. All the method will do is
        /// navigate to the change password page..
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ChangePasswordAsync()
        {
            await NavigationService.NavigateAsync("ChangePasswordPage", new NavigationParameters
            {
                { "transition-type", TransitionType.SlideFromBottom }
            });
        }
    }
}