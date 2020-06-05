using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Common;

namespace Sparky.TrakApp.ViewModel.Games
{
    /// <summary>
    ///     The <see cref="GameViewModel" /> is a simple view model that is associated with the game page view.
    ///     Its responsibility is to display all the information for a given game and respond to events such as adding,
    ///     moving or removing a game from a users' collection.
    /// </summary>
    public class GameViewModel : BaseViewModel
    {
        private readonly IRestService _restService;
        private readonly IStorageService _storageService;
        private string _description;
        private string _gameTitle;

        private Uri _gameUrl;
        private string _genreNames;

        private Uri _imageUrl;
        private bool _inLibrary;
        private string _platformNames;
        private Uri _platformUrl;
        private string _publisherNames;
        private short _rating;
        private DateTime _releaseDate;
        private ObservableCollection<ListItemViewModel> _similarGames;
        private GameUserEntryStatus _status;

        /// <summary>
        ///     Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        ///     The constructors should never be invoked outside of the Prism DI framework. All instantiation
        ///     should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService" /> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService" /> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService" /> instance to inject.</param>
        public GameViewModel(INavigationService navigationService, IStorageService storageService,
            IRestService restService) : base(navigationService)
        {
            _storageService = storageService;
            _restService = restService;

            // Default values
            Status = GameUserEntryStatus.None;

            // The page will be busy by default, as as soon as it's navigated to, API requests are made.
            IsBusy = true;
            SimilarGames = new ObservableCollection<ListItemViewModel>();
        }

        /// <summary>
        ///     A <see cref="ICommand" /> that is invoked by the view when the refresh button is tapped when an error is
        ///     thrown. When called, the command will propagate the request and call the <see cref="LoadGameInfoAsync" /> method.
        /// </summary>
        public ICommand LoadGameInfoCommand => new DelegateCommand(async () => await LoadGameInfoAsync());

        /// <summary>
        ///     A <see cref="Uri" /> that contains the URL of the image that is associated with the game within this view model.
        /// </summary>
        public Uri ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        /// <summary>
        ///     A <see cref="string" /> that contains a comma-separated list of all platforms associated with the game within
        ///     the view model.
        /// </summary>
        public string PlatformNames
        {
            get => _platformNames;
            set => SetProperty(ref _platformNames, value);
        }

        /// <summary>
        ///     A <see cref="string" /> that represents the name of the game.
        /// </summary>
        public string GameTitle
        {
            get => _gameTitle;
            set => SetProperty(ref _gameTitle, value);
        }

        /// <summary>
        ///     A <see cref="DateTime" /> that represents the release date of the game within the view model.
        /// </summary>
        public DateTime ReleaseDate
        {
            get => _releaseDate;
            set => SetProperty(ref _releaseDate, value);
        }

        /// <summary>
        ///     A <see cref="string" /> that contains a comma-separated list of all publishers associated with the game within
        ///     the view model.
        /// </summary>
        public string PublisherNames
        {
            get => _publisherNames;
            set => SetProperty(ref _publisherNames, value);
        }

        /// <summary>
        ///     A <see cref="bool" /> that is used to represent whether the game being displayed is currently within the logged in
        ///     users library. If this game is within the users library, additional information such as rating and status will
        ///     be displayed.
        /// </summary>
        public bool InLibrary
        {
            get => _inLibrary;
            set => SetProperty(ref _inLibrary, value);
        }

        /// <summary>
        ///     A <see cref="short" /> that represents the current user rating of the game. If the game being displayed isn't
        ///     within
        ///     the users collection, this value will be set to 0.
        /// </summary>
        public short Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }

        /// <summary>
        ///     A <see cref="string" /> that contains a comma-separated list of all genres associated with the game within
        ///     the view model.
        /// </summary>
        public string GenreNames
        {
            get => _genreNames;
            set => SetProperty(ref _genreNames, value);
        }

        /// <summary>
        ///     A <see cref="string" /> that represents the description of the game.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        ///     A <see cref="GameUserEntryStatus" /> that represents the current status of the game. If the game being
        ///     displayed isn't within the users collection, the status will be set to <see cref="GameUserEntryStatus.None" />.
        /// </summary>
        public GameUserEntryStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        ///     A <see cref="ObservableCollection{T}" /> that is used to represent a short collection of games that
        ///     are within the same genre as the one that is being loaded by the user.
        /// </summary>
        public ObservableCollection<ListItemViewModel> SimilarGames
        {
            get => _similarGames;
            set => SetProperty(ref _similarGames, value);
        }

        /// <summary>
        ///     Private method that is invoked by the <see cref="LoadGameInfoCommand" /> when activated by the associated
        ///     view. This method will attempt to retrieve the game information from the url provided by the
        ///     <see cref="NavigationParameters" />
        ///     and populate all of the information within this view model with data from the returned <see cref="GameInfo" />.
        ///     If any errors occur during the API requests, the exceptions are caught and the errors the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task" /> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoadGameInfoAsync()
        {
            // We're going to make some requests, so we're busy and remove any current errors.
            IsError = false;
            IsBusy = true;

            try
            {
                // Get the auth token from the store.
                var token = await _storageService.GetAuthTokenAsync();

                // Retrieve the game and set some game information on the view.
                var gameInfo = await AttemptGameInfoAsync(_gameUrl, token);

                // Load the platform data from either the supplied platform uri or the platform data with the game info.
                await AttemptPlatformDataAsync(_platformUrl, gameInfo, token);

                // Retrieve all of the genres the game is and display the names to the user.
                var genres =
                    await _restService.GetAsync<HateoasCollection<Genre>>(gameInfo.GetLink("genres").OriginalString,
                        token);

                // Grab the first genre and load some game information from it.
                var games = await _restService.GetAsync<HateoasPage<GameInfo>>(
                    genres.Embedded.Data.First().GetLink("gameInfos").OriginalString, token);

                CreateSimilarGamesList(games.Embedded.Data.Where(x => x.Id != gameInfo.Id));
            }
            catch (ApiException)
            {
                IsError = true;
            }
            catch (Exception)
            {
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        ///     Overriden method that is automatically invoked when the page is navigated to. Its purpose is to retrieve
        ///     values from the <see cref="NavigationParameters" /> before invoking <see cref="LoadGameInfoAsync" /> to
        ///     load and display additional information to the view.
        /// </summary>
        /// <param name="parameters">The <see cref="NavigationParameters" />, which contains information for display purposes.</param>
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            // Retrieve the url we're going to use to retrieve the base game data.
            _gameUrl = parameters["game-url"] as Uri;
            _platformUrl = parameters["platform-url"] as Uri;

            // This information will only be valid if the user clicked on a game entry from their own collection.
            InLibrary = parameters["in-library"] as bool? ?? false;
            Rating = parameters["rating"] as short? ?? (short) 0;
            Status = parameters["status"] as GameUserEntryStatus? ?? GameUserEntryStatus.None;

            await LoadGameInfoAsync();
        }

        /// <summary>
        ///     Private method that is invoked within the <see cref="LoadGameInfoAsync" /> method. Its purpose
        ///     is to call the <see cref="IRestService" /> to retrieve the <see cref="GameInfo" /> data and populate
        ///     the fields on the view model from the returned data.
        /// </summary>
        /// <param name="uri">The uri to call to retrieve the <see cref="GameInfo" /> data.</param>
        /// <param name="token">The authentication token to attach to the request.</param>
        /// <returns>A <see cref="Task" /> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task<GameInfo> AttemptGameInfoAsync(Uri uri, string token)
        {
            var gameInfo = await _restService.GetAsync<GameInfo>(uri.OriginalString, token);
            ImageUrl = gameInfo.GetLink("image");
            GameTitle = gameInfo.Title;
            ReleaseDate = gameInfo.ReleaseDate;
            PublisherNames = string.Join(", ", gameInfo.Publishers);
            GenreNames = string.Join(", ", gameInfo.Genres);
            Description = gameInfo.Description;

            return gameInfo;
        }

        /// <summary>
        ///     Private method that is invoked within the <see cref="LoadGameInfoAsync" /> method. Its purpose
        ///     is to convert the provided <see cref="IEnumerable{T}" /> of <see cref="GameInfo" /> instances into
        ///     <see cref="ListItemViewModel" /> instances for displaying within the list on the game page view.
        /// </summary>
        /// <param name="games">The <see cref="IEnumerable{T}" /> to convert into <see cref="ListItemViewModel" /> instances.</param>
        private void CreateSimilarGamesList(IEnumerable<GameInfo> games)
        {
            var similarGames = new ObservableCollection<ListItemViewModel>();
            foreach (var game in games)
                similarGames.Add(new ListItemViewModel
                {
                    ImageUrl = game.GetLink("image"),
                    Header = string.Join(", ", game.Platforms),
                    ItemTitle = game.Title,
                    ItemSubTitle = $"{game.ReleaseDate:MMMM yyyy}, {string.Join(", ", game.Publishers)}"
                });

            SimilarGames = similarGames;
        }

        /// <summary>
        ///     Private method that is invoked within the <see cref="LoadGameInfoAsync" /> method. Its purpose
        ///     is to load platform data either associated with the provided <see cref="GameInfo" /> instance,
        ///     or to call the <see cref="IRestService" /> to retrieve it, depending on whether the <see cref="Uri" />
        ///     is populated with a valid value. The information is then populated on the view model.
        /// </summary>
        /// <param name="uri">The uri to call to retrieve the <see cref="Platform" /> data.</param>
        /// <param name="gameInfo">The current <see cref="GameInfo" /> being displayed to the view.</param>
        /// ///
        /// <param name="token">The authentication token to attach to the request.</param>
        /// <returns>A <see cref="Task" /> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AttemptPlatformDataAsync(Uri uri, GameInfo gameInfo, string token)
        {
            if (!string.IsNullOrEmpty(uri.OriginalString))
            {
                var platform = await _restService.GetAsync<Platform>(uri.OriginalString, token);
                PlatformNames = platform.Name;
            }
            else
            {
                PlatformNames = string.Join(", ", gameInfo.Platforms);
            }
        }
    }
}