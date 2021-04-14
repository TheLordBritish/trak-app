using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using Prism.Commands;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkyStudios.TrakLibrary.Common.Extensions;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameViewModel" /> is a simple view model that is associated with the game page view.
    /// Its responsibility is to display all the information for a given game and respond to events such as adding,
    /// moving or removing a game from a users' collection.
    /// </summary>
    public class GameViewModel : ReactiveViewModel
    {
        private readonly IRestService _restService;
        private readonly IStorageService _storageService;

        private long _gameId;

        private Uri _previousGameUrl;
        
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler" /> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService" /> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService" /> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService" /> instance to inject.</param>
        public GameViewModel(IScheduler scheduler, INavigationService navigationService, IRestService restService,
            IStorageService storageService) : base(scheduler, navigationService)
        {
            _restService = restService;
            _storageService = storageService;
            
            // The page will be busy by default, as as soon as it's navigated to, API requests are made.
            SimilarGames = new ObservableCollection<ListItemViewModel>();

            OptionsCommand = ReactiveCommand.CreateFromTask(OptionsAsync, outputScheduler: scheduler);

            LoadGameDetailsCommand = ReactiveCommand.CreateFromTask(LoadGameDetailsAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            LoadGameDetailsCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                switch (ex)
                {
                    case TaskCanceledException _:
                        ErrorMessage = Messages.ErrorMessageNoInternet;
                        break;
                    case ApiException _:
                        ErrorMessage = Messages.ErrorMessageApiError;
                        break;
                    default:
                        ErrorMessage = Messages.ErrorMessageGeneric;
                        Crashes.TrackError(ex);
                        break;
                }
            });

            this.WhenAnyObservable(x => x.LoadGameDetailsCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);
        }

        /**
         * Specifies whether the page should reload the game data when navigating to. Always set to false unless
         * the user is navigating from the game options page.
         */
        public bool ShouldReload { get; set; }

        /// <summary>
        /// A <see cref="Uri"/> which specifies the URI from which the <see cref="GameDetails"/> was loaded from.
        /// </summary>
        [Reactive]
        public Uri GameUrl { get; set; }
        
        /// <summary>
        /// A <see cref="Uri" /> that contains the URL of the image that is associated with the game within this view model.
        /// </summary>
        [Reactive]
        public Uri ImageUrl { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}" /> that contains all the platforms wrapped within a <see cref="ItemEntryViewModel"/>.
        /// </summary>
        [Reactive]
        public IEnumerable<ItemEntryViewModel> Platforms { get; set; } = new List<ItemEntryViewModel>();

        /// <summary>
        /// A <see cref="string" /> that represents the name of the game.
        /// </summary>
        [Reactive]
        public string GameTitle { get; set; }

        /// <summary>
        /// A <see cref="string" /> that contains a comma-separated list of all publishers associated with the game within
        /// the view model.
        /// </summary>
        [Reactive]
        public IEnumerable<Publisher> Publishers { get; set; }

        /// <summary>
        /// A <see cref="bool" /> that is used to represent whether the game being displayed is currently within the logged in
        /// users library. If this game is within the users library, additional information such as rating and status will
        /// be displayed.
        /// </summary>
        [Reactive]
        public bool InLibrary { get; set; }

        /// <summary>
        /// A <see cref="short" /> that represents the current user rating of the game. If the game being displayed isn't
        /// within the users collection, this value will be set to 0.
        /// </summary>
        [Reactive]
        public short Rating { get; set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}" /> that contains all of the <see cref="Genre"/> objects associated with the given
        /// <see cref="Game"/>.
        /// </summary>
        [Reactive]
        public IEnumerable<Genre> Genres { get; set; }

        /// <summary>
        /// A <see cref="string" /> that represents the description of the game.
        /// </summary>
        [Reactive]
        public string Description { get; set; }

        /// <summary>
        /// A <see cref="string"/> that represents the North American release date for the <see cref="Game"/>.
        /// </summary>
        [Reactive] 
        public string NorthAmericaReleaseDate { get; set; } = Messages.GamePageReleaseDateNotReleased;

        /// <summary>
        /// A <see cref="string"/> that represents the European release date for the <see cref="Game"/>.
        /// </summary>
        [Reactive] 
        public string EuropeReleaseDate { get; set; } = Messages.GamePageReleaseDateNotReleased;

        /// <summary>
        /// A <see cref="string"/> that represents the Japanese release date for the <see cref="Game"/>.
        /// </summary>
        [Reactive] 
        public string JapanReleaseDate { get; set; } = Messages.GamePageReleaseDateNotReleased;

        /// <summary>
        /// The title of the <see cref="Franchise"/> of the currently viewed <see cref="Game"/>. Set to
        /// <code>null</code> if the <see cref="Game"/> isn't in a <see cref="Franchise"/>.
        /// </summary>
        [Reactive]
        public string FranchiseTitle { get; set; }
        
        /// <summary>
        /// An <see cref="string"/> that contains a comma-separated value of all of the <see cref="GameModes"/> that the
        /// selected <see cref="Game"/> has.
        /// </summary>
        [Reactive]
        public string GameModes { get; set; }
        
        /// <summary>
        /// A <see cref="GameUserEntryStatus" /> that represents the current status of the game. If the game being
        /// displayed isn't within the users collection, the status will be set to <see cref="GameUserEntryStatus.Backlog" />.
        /// </summary>
        [Reactive]
        public GameUserEntryStatus Status { get; private set; }

        /// <summary>
        /// The current assigned <see cref="AgeRating"/> of the <see cref="Game"/>.
        /// </summary>
        [Reactive]
        public AgeRating AgeRating { get; private set; }
        
        /// <summary>
        /// A <see cref="ObservableCollection{T}" /> that is used to represent a short collection of games that
        /// are within the same genre as the one that is being loaded by the user.
        /// </summary>
        [Reactive]
        public ObservableCollection<ListItemViewModel> SimilarGames { get; set; }

        /// <summary>
        /// Command that is invoked each time the Add or options button is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="OptionsAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OptionsCommand { get; }

        /// <summary>
        /// Command that is invoked each time the page is first navigated to. When called, the command will
        /// propagate the request and call the <see cref="LoadGameDetailsAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadGameDetailsCommand { get; }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back && _previousGameUrl != null)
            {
                parameters.Add("game-url", _previousGameUrl);
                parameters.Add("reload", true);
            }
        }

        /// <summary>
        /// Overriden method that is automatically invoked when the page is navigated to. Its purpose is to retrieve
        /// values from the <see cref="NavigationParameters" /> before invoking <see cref="LoadGameDetailsAsync" /> to
        /// load and display additional information to the view.
        /// </summary>
        /// <param name="parameters">The <see cref="NavigationParameters" />, which contains information for display purposes.</param>
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _previousGameUrl = parameters.GetValue<Uri>("previous-game-url");
            var reload = parameters.GetValue<bool>("reload");
            
            // Retrieve the url we're going to use to retrieve the base game data.
            ShouldReload = parameters.GetNavigationMode() == NavigationMode.New ||
                           parameters.GetNavigationMode() == NavigationMode.Forward || reload;
            
            GameUrl = parameters.GetValue<Uri>("game-url");
            Rating = parameters.GetValue<short>("rating");
            Status = parameters.GetValue<GameUserEntryStatus>("status");
            InLibrary = parameters.GetValue<bool>("in-library");
                
            // They'll only be selected platform ID's if the page is being navigated to from the game
            // options page. 
            var selectedPlatformIds = parameters.GetValue<IEnumerable<long>>("selected-platforms");
            UpdateSelectedPlatforms(Platforms, selectedPlatformIds ?? Enumerable.Empty<long>());

            // Load the game immediately on navigation.
            LoadGameDetailsCommand.Execute()
                .Catch(Observable.Return(Unit.Default))
                .Subscribe();
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="OptionsCommand"/>. When invoked, it will navigate the user to the
        /// game options page.
        /// or not.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task OptionsAsync()
        {
            await NavigationService.NavigateAsync("GameOptionsPage", new NavigationParameters
            {
                {"game-url", GameUrl},
                {"game-id", _gameId}
            });
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
            // If we're coming from the game options page, there's no point in reloading the whole page, only change
            // what the game options page changed.
            if (!ShouldReload)
            {
                return;
            }

            // Remove any previous error messages before loading.
            IsError = false;
            HasLoaded = false;
 
            // Retrieve the game and set some game information on the view.
            var gameDetails = await _restService.GetAsync<GameDetails>(GameUrl.OriginalString);
            _gameId = gameDetails.Id;
            ImageUrl = gameDetails.GetLink("medium_image");
            GameTitle = gameDetails.Title;
            Description = gameDetails.Description;
            Publishers = gameDetails.Publishers;
            Genres = gameDetails.Genres;
            FranchiseTitle = gameDetails.Franchise?.Title;
            GameModes = string.Join(", ", gameDetails.GameModes
                .Select(gameMode => gameMode.GetAttributeValue<DescriptionAttribute, string>(x => x.Description))
                .ToArray());

            AgeRating = gameDetails.AgeRating;
            
            var platforms = gameDetails.Platforms.Select(p => new ItemEntryViewModel
            {
                Id = p.Id,
                Name = p.Name
            }).ToList();
            
            // Set the release dates (if there are any).
            foreach (var releaseDate in gameDetails.ReleaseDates)
            {
                var date = releaseDate.ReleaseDate.ToString("dd MMMM yyyy");
                switch (releaseDate.Region)
                {
                    case GameRegion.NorthAmerica:
                        NorthAmericaReleaseDate = date;
                        break;

                    case GameRegion.Pal:
                        EuropeReleaseDate = date;
                        break;

                    case GameRegion.Japan:
                        JapanReleaseDate = date;
                        break;
                }
            }

            // Grab the first genre and load some game information from it.
            var enumerable = Genres as Genre[] ?? Genres.ToArray();
            if (enumerable.Any())
            {
                var genre = enumerable.First();
                var games = await _restService.GetAsync<HateoasPage<GameDetails>>(
                    $"games/genres/{genre.Id}/games/details");
                CreateSimilarGamesList(games.Embedded.Data.Where(x => x.Id != gameDetails.Id));
            }

            // Get the ID of the user currently logged in.
            var userId = await _storageService.GetUserIdAsync();

            // See if there is an existing entry for this game in the users collection.
            var gameUserEntries =
                await _restService.GetAsync<HateoasPage<GameUserEntry>>($"{gameDetails.GetLink("entries")}?user-id={userId}");

            // See if the game is already in the users collection.
            if (gameUserEntries.Embedded != null && gameUserEntries.Embedded.Data.Any())
            {
                InLibrary = true;
                var entry = gameUserEntries.Embedded.Data.First();

                // Set the values on the page to those found within the current game user entry.
                Status = entry.Status;
                Rating = entry.Rating;

                UpdateSelectedPlatforms(platforms, entry.GameUserEntryPlatforms.Select(g => g.PlatformId).ToList());
            }
            else
            {
                UpdateSelectedPlatforms(platforms, Enumerable.Empty<long>());
            }
            
            HasLoaded = true;
        }

        /// <summary>
        /// Private method that is invoked within the <see cref="LoadGameDetailsAsync" /> method. Its purpose
        /// is to convert the provided <see cref="IEnumerable{T}" /> of <see cref="GameDetails" /> instances into
        /// <see cref="ListItemViewModel" /> instances for displaying within the list on the game page view.
        /// </summary>
        /// <param name="games">The <see cref="IEnumerable{T}" /> to convert into <see cref="ListItemViewModel" /> instances.</param>
        private void CreateSimilarGamesList(IEnumerable<GameDetails> games)
        {
            var similarGames = new ObservableCollection<ListItemViewModel>();
            foreach (var game in games)
                similarGames.Add(new ListItemViewModel
                {
                    ImageUrl = game.GetLink("small_image"),
                    HeaderDetails = game.Platforms.Select(x => new ItemEntryViewModel
                    {
                        Name = x.Name,
                        IsSelected = true
                    }).OrderBy(x => x.Name).ToList(),
                    ItemTitle = game.Title,
                    ItemSubTitle = string.Join(", ", game.Publishers.Select(x => x.Name)),
                    TapCommand = new DelegateCommand(async () =>
                    {
                        var parameters = new NavigationParameters
                        {
                            {"game-url", game.GetLink("self")},
                            {"previous-game-url", GameUrl}
                        };

                        await NavigationService.NavigateAsync("GamePage", parameters);
                    })
                });

            SimilarGames = similarGames;
        }

        private void UpdateSelectedPlatforms(IEnumerable<ItemEntryViewModel> platforms, IEnumerable<long> selectedPlatformIds)
        {
            foreach (var platform in platforms)
            {
                platform.HasNext = false;
                var platformIds = selectedPlatformIds as long[] ?? selectedPlatformIds.ToArray();
                platform.IsSelected = platformIds.Contains(platform.Id);
            }

            var sortedPlatforms = platforms.OrderBy(x => !x.IsSelected)
                .ThenBy(x => x.Name)
                .ToList();
            
            for (var i = 0; i < sortedPlatforms.Count - 1; i++)
            {
                sortedPlatforms.ElementAt(i).HasNext = true;
            }

            Platforms = sortedPlatforms;
        }
    }
}