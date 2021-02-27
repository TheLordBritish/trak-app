using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Prism.Navigation;
using ReactiveUI;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.ViewModel.Common;

namespace SparkyStudios.TrakLibrary.ViewModel
{
    /// <summary>
    /// The <see cref="BaseFlyoutViewModel" /> is a simple view model that is associated with the master details view.
    /// Its responsibility is to respond to the user tapping on any of the subcategories displayed within the master details
    /// view, such as games, settings or logout.
    /// </summary>
    public class BaseFlyoutViewModel : ReactiveViewModel
    {
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler" /> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService" /> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService" /> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService" /> instance to inject.</param>
        public BaseFlyoutViewModel(IScheduler scheduler, INavigationService navigationService,
            IStorageService storageService, IRestService restService) : base(scheduler, navigationService)
        {
            _storageService = storageService;
            _restService = restService;

            LoadHomeCommand = ReactiveCommand.CreateFromTask(LoadHomeAsync, outputScheduler: scheduler);
            LoadGamesCommand = ReactiveCommand.CreateFromTask(LoadGamesAsync, outputScheduler: scheduler);
            LoadSettingsCommand = ReactiveCommand.CreateFromTask(LoadSettingsAsync, outputScheduler: scheduler);
            
            LogoutCommand = ReactiveCommand.CreateFromTask(LogoutAsync, outputScheduler: scheduler);
            // Not much if the logout fails, just still go to the login page.
            LogoutCommand.ThrownExceptions.Subscribe(async ex =>
            {
                // Navigate back to the login page.
                await NavigationService.NavigateAsync("/LoginPage");
            });
        }

        /// <summary>
        /// Command that is invoked each time the home label is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="LoadHomeAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadHomeCommand { get; }

        /// <summary>
        /// Command that is invoked each time the games label is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="LoadGamesAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadGamesCommand { get; }

        /// <summary>
        /// Command that is invoked each time the settings label is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="LoadSettingsAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadSettingsCommand { get; }
        
        /// <summary>
        /// Command that is invoked each time the logout label is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="LogoutAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoadHomeCommand"/>. When called, it will navigate
        /// the user to the home page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoadHomeAsync()
        {
            await NavigationService.NavigateAsync("NavigationPage/HomePage");
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoadGamesCommand"/>. When called, it will navigate
        /// the user to the games page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoadGamesAsync()
        {
            await NavigationService.NavigateAsync("NavigationPage/GameUserEntriesTabbedPage");
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoadSettingsCommand"/>. When called, it will navigate
        /// the user to the games page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoadSettingsAsync()
        {
            await NavigationService.NavigateAsync("NavigationPage/SettingsPage");
        }
        
        /// <summary>
        /// Private method that invoked by the <see cref="LogoutCommand"/>. When called, it will remove all of the
        /// personal identifiable information from the <see cref="IStorageService"/> and remove the notification token
        /// from the push notification service.
        ///
        /// Once the information has been removed, the user will be redirected back to the login page and reset the
        /// navigation stack.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LogoutAsync()
        {
            // Get some values first before removing them all from the secure storage.
            var userId = await _storageService.GetUserIdAsync();
            var deviceId = await _storageService.GetDeviceIdAsync();

            // Need to ensure the correct details are registered for push notifications.
            await _restService.DeleteAsync(
                $"notifications/unregister?user-id={userId}&device-guid={deviceId}");

            // Remove all of the identifiable information from the secure store.
            await _storageService.ClearCredentialsAsync();

            // Navigate back to the login page.
            await NavigationService.NavigateAsync("/LoginPage");
        }
    }
}