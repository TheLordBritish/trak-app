using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.AppCenter.Crashes;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Games.Request;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameOptionsViewModel" /> is a simple view model that is associated with the game options page view.
    /// Its responsibility is to allow users to add, update and delete games to and from their own collection.
    /// </summary>
    public class GameOptionsViewModel : ReactiveViewModel
    {
        private readonly IRestService _restService;
        private readonly IStorageService _storageService;
        private readonly IUserDialogs _userDialogs;

        private long _gameId;
        private long _gameUserEntryId;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler" /> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService" /> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService" /> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService" /> instance to inject.</param>
        /// <param name="userDialogs">The <see cref="IUserDialogs" /> instance to inject.</param>
        public GameOptionsViewModel(IScheduler scheduler, INavigationService navigationService,
            IRestService restService, IStorageService storageService, IUserDialogs userDialogs) : base(scheduler,
            navigationService)
        {
            _restService = restService;
            _storageService = storageService;
            _userDialogs = userDialogs;

            LoadGameDetailsCommand = ReactiveCommand.CreateFromTask(LoadGameDetailsAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            LoadGameDetailsCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (!(ex is ApiException))
                {
                    Crashes.TrackError(ex);
                }
            });

            OnPlatformTappedCommand = ReactiveCommand.Create<ItemEntryViewModel>(item =>
            {
                ErrorMessage = string.Empty;
                if (item == null) return;
                
                var index = Platforms.IndexOf(item);
                    
                item.IsSelected = !item.IsSelected;
                Platforms[index] = item;
            });
            OnRatingTappedCommand = ReactiveCommand.Create<string>(rating =>
            {
                ErrorMessage = string.Empty;
                Rating = short.Parse(rating);
            });
            
            OnRatingRemovedCommand = ReactiveCommand.Create<short>(s =>
            {
                ErrorMessage = string.Empty;
                Rating = 0;
            });
            
            OnStatusTappedCommand = ReactiveCommand.Create<GameUserEntryStatus>(status =>
            {
                ErrorMessage = string.Empty;
                Status = status;
            });

            AddGameCommand = ReactiveCommand.CreateFromTask(AddGameAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            AddGameCommand.ThrownExceptions.Subscribe(ex =>
            {
                if (ex is ApiException)
                {
                    ErrorMessage = Messages.ErrorMessageApiError;
                }
                else
                {
                    ErrorMessage = Messages.ErrorMessageGeneric;
                    Crashes.TrackError(ex, new Dictionary<string, string>
                    {
                        {"Game ID", _gameId.ToString()}
                    });
                }
            });

            UpdateGameCommand = ReactiveCommand.CreateFromTask(UpdateGameAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            UpdateGameCommand.ThrownExceptions.Subscribe(ex =>
            {
                if (ex is ApiException)
                {
                    ErrorMessage = Messages.ErrorMessageApiError;
                }
                else
                {
                    ErrorMessage = Messages.ErrorMessageGeneric;
                    Crashes.TrackError(ex, new Dictionary<string, string>
                    {
                        {"Game ID", _gameId.ToString()},
                        {"Game User Entry ID", _gameUserEntryId.ToString()}
                    });
                }
            });

            DeleteGameCommand = ReactiveCommand.CreateFromTask(DeleteGameAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            DeleteGameCommand.ThrownExceptions.Subscribe(ex =>
            {
                if (ex is ApiException)
                {
                    ErrorMessage = Messages.ErrorMessageApiError;
                }
                else
                {
                    ErrorMessage = Messages.ErrorMessageGeneric;
                    Crashes.TrackError(ex, new Dictionary<string, string>
                    {
                        {"Game ID", _gameId.ToString()},
                        {"Game User Entry ID", _gameUserEntryId.ToString()}
                    });
                }
            });

            this.WhenAnyObservable(x => x.LoadGameDetailsCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);

            this.WhenAnyObservable(x => x.AddGameCommand.IsExecuting, x => x.UpdateGameCommand.IsExecuting,
                    x => x.DeleteGameCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsExecutingLibraryRequest, scheduler: scheduler);
        }

        /**
         * The URL that is called to retrieve the information for the given game that these options are displaying
         * data for.
         */
        public Uri GameUrl { get; set; }

        /// <summary>
        /// A <see cref="bool"/> that specifies whether any game user entries are currently being made.
        /// </summary>
        public bool IsExecutingLibraryRequest { [ObservableAsProperty] get; }

        /// <summary>
        /// A <see cref="bool" /> that is used to represent whether the options for the current game being displayed is within
        /// the logged in users library. If this game is within the users library, it will allow the user to update or remove
        /// a game from their collection.
        /// </summary>
        [Reactive]
        public bool InLibrary { get; set; }

        /// <summary>
        /// A <see cref="string" /> that represents the name of the game.
        /// </summary>
        [Reactive]
        public string GameTitle { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}" /> that contains all the platforms for the given game.
        /// </summary>
        [Reactive]
        public ObservableCollection<ItemEntryViewModel> Platforms { get; set; } = new ObservableCollection<ItemEntryViewModel>();

        /// <summary>
        /// A <see cref="GameUserEntryStatus"/> that represents the currently selected status. Set to
        /// <code>null</code> by default.
        /// </summary>
        [Reactive]
        public GameUserEntryStatus Status { get; set; }

        /// <summary>
        /// A <see cref="short" /> that represents the current user rating of the game. If the game hasn't yet been
        /// rated, it'll be set to 0.
        /// </summary>
        [Reactive]
        public short Rating { get; set; }

        /// <summary>
        /// Command that is invoked each a platform is tapped on the view. When called, it will reserve the selected
        /// state of the given <see cref="ItemEntryViewModel"/>.
        /// </summary>
        public ReactiveCommand<ItemEntryViewModel, Unit> OnPlatformTappedCommand { get; }
        
        /// <summary>
        /// Command that is invoked each time the page is first navigated to. When called, the command will
        /// propagate the request and call the <see cref="LoadGameDetailsAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadGameDetailsCommand { get; }

        /// <summary>
        /// Command that is invoked each time any of the rating stars are tapped by the user. When called,
        /// it will update the rating to the value provided.
        /// </summary>
        public ReactiveCommand<string, Unit> OnRatingTappedCommand { get; }

        /// <summary>
        /// Command that is invoked each time the user taps the "Remove Rating" label. When called,
        /// it will update the rating and set it to 0.
        /// </summary>
        public ReactiveCommand<short, Unit> OnRatingRemovedCommand { get; }
        
        /// <summary>
        /// Command that is invoked when the user attempts to add a game to their library. When called,
        /// the command will propagate the request and call the <see cref="AddGameAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddGameCommand { get; }

        /// <summary>
        /// Command that is invoked when the user attempts to update a game within their library. When called,
        /// the command will propagate the request and call the <see cref="UpdateGameAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> UpdateGameCommand { get; }

        /// <summary>
        /// Command that is invoked when the user attempts to delete a game within their library. When called,
        /// the command will propagate the request and call the <see cref="DeleteGameAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteGameCommand { get; }

        /// <summary>
        /// Command that is invoked each time any of the statuses are tapped by the user. When called,
        /// it will update the status to the value provided.
        /// </summary>
        public ReactiveCommand<GameUserEntryStatus, Unit> OnStatusTappedCommand { get; }

        /// <summary>
        /// Overriden method that is automatically invoked when the page is being navigated away from. Its purpose is to
        /// supply the <see cref="GameViewModel"/> with some additional values to display any changes in information without
        /// having to reload data from the server.
        /// </summary>
        /// <param name="parameters">The <see cref="NavigationParameters" />, which contains information for display purposes.</param>
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            parameters.Add("game-url", GameUrl);
            parameters.Add("game-id", _gameId);
            parameters.Add("in-library", InLibrary);
            parameters.Add("status", Status);
            parameters.Add("rating", Rating);
            parameters.Add("selected-platforms",
                InLibrary ? Platforms.Where(x => x.IsSelected).Select(x => x.Id).ToList() : Enumerable.Empty<long>());
        }

        /// <summary>
        /// Overriden method that is automatically invoked when the page is navigated to. Its purpose is to retrieve
        /// values from the <see cref="NavigationParameters" /> before invoking <see cref="LoadGameDetailsAsync" /> to
        /// load and display additional information to the view.
        /// </summary>
        /// <param name="parameters">The <see cref="NavigationParameters" />, which contains information for display purposes.</param>
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            // Retrieve the url we're going to use to retrieve the base game data.
            GameUrl = parameters.GetValue<Uri>("game-url");
            _gameId = parameters.GetValue<long>("game-id");

            LoadGameDetailsCommand.Execute().Subscribe();
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoadGameDetailsCommand" /> when activated by the associated
        /// view. This method will attempt to retrieve the game information from the url provided by the
        /// <see cref="NavigationParameters" /> and populate all of the information within this view model with data
        /// from the returned <see cref="GameDetails" />. If any errors occur during the API requests, the exceptions
        /// are caught and the errors the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task" /> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoadGameDetailsAsync()
        {
            // We're going to make some requests, so we're busy and remove any current errors.
            IsError = false;
            HasLoaded = false;

            // Retrieve the game and set some game information on the view.
            var gameDetails = await _restService.GetAsync<GameDetails>(GameUrl.OriginalString);
            GameTitle = gameDetails.Title;

            var platformSelections = gameDetails.Platforms.Select(p => new ItemEntryViewModel
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();

            // Get the ID of the user currently logged in.
            var userId = await _storageService.GetUserIdAsync();

            // See if there is an existing entry for this game in the users collection.
            var gameUserEntries =
                await _restService.GetAsync<HateoasPage<GameUserEntry>>(gameDetails.GetLink("entries") +
                                                                        $"?game-id={_gameId}&user-id={userId}");

            // There should only be a single entry for this.
            if (gameUserEntries.Embedded != null && gameUserEntries.Embedded.Data.Any())
            {
                InLibrary = true;
                var entry = gameUserEntries.Embedded.Data.First();

                // Set the values on the page to those found within the current game user entry.
                _gameUserEntryId = entry.Id;
                Status = entry.Status;
                Rating = entry.Rating;

                foreach (var gameUserEntryPlatform in entry.GameUserEntryPlatforms)
                {
                    foreach (var platform in platformSelections.Where(platform =>
                        platform.Id == gameUserEntryPlatform.PlatformId))
                    {
                        platform.IsSelected = true;
                    }
                }
            }

            Platforms = new ObservableCollection<ItemEntryViewModel>(platformSelections);
            HasLoaded = true;
        }

        private async Task AddGameAsync()
        {
            // Reset the error message before attempting to save.
            ErrorMessage = string.Empty;

            if (!Platforms.Any(p => p.IsSelected))
            {
                ErrorMessage = Messages.GameOptionsPageNoSelectedPlatforms;
                return;
            }

            if (Status == GameUserEntryStatus.None)
            {
                ErrorMessage = Messages.GameOptionsPageNoSelectedStatus;
                return;
            }

            await AttemptAddGameAsync();
        }

        private async Task AttemptAddGameAsync()
        {
            // Get the ID of the user currently logged in.
            var userId = await _storageService.GetUserIdAsync();

            // Make a request to save the user entry.
            var request = new GameUserEntryRequest
            {
                UserId = userId,
                GameId = _gameId,
                Rating = Rating,
                Status = Status,
                PlatformIds = Platforms.Where(p => p.IsSelected).Select(p => p.Id).ToList()
            };

            var result = await _restService.PostAsync<GameUserEntry, GameUserEntryRequest>("games/entries", request);
            Rating = result.Rating;
            Status = result.Status;

            // Once the element has been saved, mark it as in the users library and go back.
            InLibrary = true;
            await NavigationService.GoBackAsync();

            var alertConfig = new AlertConfig()
                .SetTitle(Messages.TrakTitle)
                .SetMessage(string.Format(Messages.GameOptionsPageGameAdded, GameTitle));

            await _userDialogs.AlertAsync(alertConfig);
        }

        private async Task UpdateGameAsync()
        {
            // Reset the error message before attempting to update.
            ErrorMessage = string.Empty;

            if (!Platforms.Any(p => p.IsSelected))
            {
                ErrorMessage = Messages.GameOptionsPageNoSelectedPlatforms;
                return;
            }

            await AttemptUpdateGameAsync();
        }

        private async Task AttemptUpdateGameAsync()
        {
            // Get the ID of the user currently logged in.
            var userId = await _storageService.GetUserIdAsync();

            // Make a request to update the user entry.
            var request = new GameUserEntryRequest
            {
                GameUserEntryId = _gameUserEntryId,
                UserId = userId,
                GameId = _gameId,
                Rating = Rating,
                Status = Status,
                PlatformIds = Platforms.Where(p => p.IsSelected).Select(p => p.Id).ToList()
            };

            var result = await _restService.PutAsync<GameUserEntry, GameUserEntryRequest>("games/entries", request);
            Rating = result.Rating;
            Status = result.Status;

            // Once the element has been updated, go back to the game page.
            InLibrary = true;
            await NavigationService.GoBackAsync();
        }

        private async Task DeleteGameAsync()
        {
            await _restService.DeleteAsync($"games/entries/{_gameUserEntryId}");

            Rating = 0;
            Status = GameUserEntryStatus.None;
            InLibrary = false;

            // Once the element has been deleted, go back to the game page.
            await NavigationService.GoBackAsync();

            var alertConfig = new AlertConfig()
                .SetTitle(Messages.TrakTitle)
                .SetMessage(string.Format(Messages.GameOptionsPageGameRemoved, GameTitle));

            await _userDialogs.AlertAsync(alertConfig);
        }
    }
}