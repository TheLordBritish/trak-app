using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Common;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Games
{
    public class GameOptionsViewModel : ReactiveViewModel
    {
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;
        
        private bool _inLibrary = true;

        private GameUserEntryStatus _originalStatus;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        public GameOptionsViewModel(IScheduler scheduler, INavigationService navigationService, IStorageService storageService, IRestService restService) : base(scheduler, navigationService)
        {
            _storageService = storageService;
            _restService = restService;
            
            Statuses = new ObservableCollection<GameUserEntryStatus>
            {
                GameUserEntryStatus.Backlog,
                GameUserEntryStatus.InProgress,
                GameUserEntryStatus.Completed,
                GameUserEntryStatus.Dropped
            };
            
            UpdateGameCommand = ReactiveCommand.CreateFromTask(ExecuteUpdateGameAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            UpdateGameCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                ErrorMessage = ex is ApiException ? Messages.ErrorMessageApiError : Messages.ErrorMessageGeneric;
            });
            
            DeleteGameCommand = ReactiveCommand.CreateFromTask(ExecuteDeleteGameAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            DeleteGameCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                ErrorMessage = ex is ApiException ? Messages.ErrorMessageApiError : Messages.ErrorMessageGeneric;
            });
            
            this.WhenAnyObservable(x => x.UpdateGameCommand.IsExecuting, x => x.DeleteGameCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);
        }
        
        public Uri GameUrl { get; private set; }
        
        public long GameId { get; private set; }
        
        public long PlatformId { get; private set; }
        
        public bool IsUpdated { get; private set; }
        
        /// <summary>
        /// An <see cref="ObservableCollection{T}"/> that contains a list of all valid and selectable
        /// <see cref="GameUserEntryStatus"/> enumerations.
        /// </summary>
        [Reactive]
        public ObservableCollection<GameUserEntryStatus> Statuses { get; private set; }

        /// <summary>
        /// A <see cref="GameUserEntryStatus"/> that contains the currently selected status.
        /// </summary>
        [Reactive]
        public GameUserEntryStatus SelectedStatus { get; set; }
        
        /// <summary>
        /// Command that is invoked each the time the Update button is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="ExecuteUpdateGameAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> UpdateGameCommand { get; }
        
        /// <summary>
        /// Command that is invoked each the time the Delete button is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="ExecuteDeleteGameAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteGameCommand { get; }

        /// <summary>
        /// Invoked when the view associated with this view model is being removed from the view. It will add
        /// a number of parameters to the <see cref="INavigationParameters"/> instance provided referencing the
        /// game selected, as this information is needed when navigating back to the game view.
        /// </summary>
        /// <param name="parameters">The <see cref="INavigationParameters"/> sent when navigating away.</param>
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            parameters.Add("game-url", GameUrl);
            parameters.Add("platform-id", PlatformId);
            parameters.Add("in-library", _inLibrary);
            parameters.Add("status", _inLibrary && IsUpdated ? SelectedStatus : _originalStatus);
        }
        
        /// <summary>
        /// Invoked when the view first navigates to the view associated with this view model. It will retrieve
        /// the game information provided by the <see cref="INavigationParameters"/> and the associated platform ID
        /// of the <see cref="Game"/> and its current status within the users collection.
        /// </summary>
        /// <param name="parameters">The <see cref="INavigationParameters"/> sent when navigated to.</param>
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            GameUrl = parameters.GetValue<Uri>("game-url");
            GameId = parameters.GetValue<long>("game-id");
            PlatformId = parameters.GetValue<long>("platform-id");
            _originalStatus = parameters.GetValue<GameUserEntryStatus>("status");
            SelectedStatus = _originalStatus;
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="UpdateGameCommand"/> when activated by the associated
        /// view. This method will attempt to find the entry that matches the selected criteria and update it with the
        /// new status that the user has provided. Once updated, the user will be redirected back to the game view. 
        ///
        /// If any errors occur, the exceptions are caught and the errors are displayed to the user through the
        /// ErrorMessage parameter and setting the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ExecuteUpdateGameAsync()
        {
            IsError = false;

            // Get the needed values to make the call from the storage service.
            var userId = await _storageService.GetUserIdAsync();
            var token = await _storageService.GetAuthTokenAsync();

            // Make the request to see if the game they're adding is already in their library.
            var existingEntry =
                await _restService.GetAsync<HateoasPage<GameUserEntry>>($"api/game-management/v1/game-user-entries?user-id={userId}&platform-id={PlatformId}&game-id={GameId}",
                    token);

            if (existingEntry.Embedded != null)
            {
                // If the entry is already in the users library, just update it with the selected status they provided.
                var entry = existingEntry.Embedded.Data.First();
                entry.Status = SelectedStatus;
                
                // Make a request to update the game to their collection.
                await _restService.PutAsync("api/game-management/v1/game-user-entries", entry, token);
            }

            IsUpdated = true;
            await NavigationService.GoBackAsync();
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="DeleteGameCommand"/> when activated by the associated
        /// view. This method will attempt to find the entry that matches the selected criteria and remove it from the
        /// users collection. Once removed, the user will be redirected back to the game view.
        ///
        /// If any errors occur, the exceptions are caught and the errors are displayed to the user through the
        /// ErrorMessage parameter and setting the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ExecuteDeleteGameAsync()
        {
            IsError = false;
            
            // Get the needed values to make the call from the storage service.
            var userId = await _storageService.GetUserIdAsync();
            var token = await _storageService.GetAuthTokenAsync();

            // Make the request to see if the game they're adding is already in their library.
            var existingEntry =
                await _restService.GetAsync<HateoasPage<GameUserEntry>>($"api/game-management/v1/game-user-entries?user-id={userId}&platform-id={PlatformId}&game-id={GameId}",
                    token);

            if (existingEntry.Embedded != null)
            {
                // Make a request to delete the game from their collection.
                await _restService.DeleteAsync($"api/game-management/v1/game-user-entries/{existingEntry.Embedded.Data.First().Id}", token);
            }

            _inLibrary = false;
            await NavigationService.GoBackAsync();
        }
    }
}