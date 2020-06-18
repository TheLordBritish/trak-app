using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Common;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameLibraryListViewModel"/> is the view model that is associated with the game library list page view.
    /// Its responsibility is to respond to user defined search queries and display a list of <see cref="GameInfo"/> that
    /// match the given criteria. The user can click on entries from here to add them to their own library.
    ///
    /// If the user searches for a game that isn't in the library, this view model also allows them to make requests
    /// for the game to be added to the library.
    /// </summary>
    public class GameLibraryListViewModel : BaseListViewModel<ListItemViewModel>
    {
        private readonly IRestService _restService;
        private readonly IStorageService _storageService;
        private readonly IUserDialogs _userDialogs;

        private string _nextUri;
        private string _search;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// /// <param name="userDialogs">The <see cref="IUserDialogs"/> instance to inject.</param>
        public GameLibraryListViewModel(INavigationService navigationService, IRestService restService,
            IStorageService storageService, IUserDialogs userDialogs) : base(navigationService)
        {
            _restService = restService;
            _storageService = storageService;
            _userDialogs = userDialogs;
            
            RefreshCommand = new DelegateCommand<object>(async clear => await RefreshAsync(clear), (a) => !IsRefreshing);
            LoadMoreCommand = new DelegateCommand(async () => await LoadGamesNextPageAsync(),
                () => !IsBusy && _nextUri != null);
        }
        
        public ICommand RequestCommand => new DelegateCommand(async () => await RequestAsync());
        
        /// <summary>
        /// A <see cref="string"/> that contains the currently populated user defined query to search
        /// the game library with.
        /// </summary>
        public string Search
        {
            get => _search;
            set => SetProperty(ref _search, value);
        }

        /// <summary>
        /// Private method that is invoked when the <see cref="RefreshCommand"/> is called by the view. It's
        /// purpose is to load the first page of <see cref="GameInfo"/> objects from the server and convert
        /// them into <see cref="ListItemViewModel"/> instances for displaying within a collection view. If
        /// any errors occur during loading, then the <see cref="ErrorMessage"/> variable will be populated
        /// with additional information.
        /// </summary>
        /// <param name="clear">A <see cref="bool"/> that can be used to clear the existing list before loading the first page.</param>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task RefreshAsync(object clear)
        {
            if (!string.IsNullOrEmpty(Search))
            {
                // Define the first page when searching, it'll be set to null if there are no more pages 
                // to load.
                _nextUri = $"api/game-management/v1/games/info?title={Search}&sort=title";
                
                // Check whether we want to clear the current results before loading more.
                if (bool.TryParse(clear.ToString(), out var result) && result)
                {
                    Items.Clear();
                }

                IsError = false;
                IsBusy = true;
                IsRefreshing = true;

                try
                {
                    // Retrieve all of the games for the current page and turn them into
                    // list item view models.
                    Items.AddRange((await GetGamesFromUrlAsync(_nextUri))
                        .Select(CreateListItemViewModelFromGameInfo));
                    
                    // If it's empty, we'll display a message to the user and ask if they want 
                    // a game added to the library.
                    IsEmpty = Items.Count == 0;
                }
                catch (ApiException)
                {
                    IsError = true;
                    ErrorMessage = Messages.GameLibraryListPageEmptyServerError;
                }
                catch (Exception)
                {
                    IsError = true;
                    ErrorMessage = Messages.GameLibraryListPageEmptyGenericError;
                }
                finally
                {
                    IsBusy = false;
                    IsRefreshing = false;
                }
            }
        }

        private async Task RequestAsync()
        {
            await NavigationService.NavigateAsync("GameRequestPage");
        } 

        /// <summary>
        /// Private method that is invoked when the <see cref="LoadMoreCommand"/> is called by the view. It's
        /// purpose is to load an additional page of <see cref="GameInfo"/> objects from the server and convert
        /// them into <see cref="ListItemViewModel"/> instances for displaying within a collection view. If
        /// any errors occur during loading, then toast message will be pushed to the view.
        ///
        /// It should be noted that this command will only be invoked if the view is not currently busy and
        /// the next Uri is not set to a null value.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoadGamesNextPageAsync()
        {
            if (!string.IsNullOrEmpty(Search))
            {
                IsError = false;
                IsBusy = true;

                try
                {
                    // Load the next page of game user entries with the next page URL.
                    Items.AddRange((await GetGamesFromUrlAsync(_nextUri))
                        .Select(CreateListItemViewModelFromGameInfo));
                }
                catch (ApiException)
                {
                    IsError = true;
                    _userDialogs.Toast(new ToastConfig(Messages.GameLibraryListPageEmptyServerError)
                        .SetBackgroundColor(Color.Red)
                        .SetMessageTextColor(Color.White)
                        .SetDuration(TimeSpan.FromSeconds(5))
                        .SetPosition(ToastPosition.Bottom));
                }
                catch (Exception)
                {
                    IsError = true;
                    _userDialogs.Toast(new ToastConfig(Messages.GameLibraryListPageEmptyGenericError)
                        .SetBackgroundColor(Color.Red)
                        .SetMessageTextColor(Color.White)
                        .SetDuration(TimeSpan.FromSeconds(5))
                        .SetPosition(ToastPosition.Bottom));
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Retrieves an <see cref="IEnumerable{T}"/> of <see cref="GameInfo"/> objects from the server by
        /// invoking the given url. No error checking is done within this method, it is done by the private
        /// methods that invoke it.
        ///
        /// If the data loaded has an additional page, the next URI is set and more data can be loaded when
        /// the user reaches the end of the collection.
        /// </summary>
        /// <param name="url">The url to invoke to retrieve another page of <see cref="GameInfo"/> instances.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="GameInfo"/> objects from the given page.</returns>
        private async Task<IEnumerable<GameInfo>> GetGamesFromUrlAsync(string url)
        {
            // Get the needed variables from the store.
            var authToken = await _storageService.GetAuthTokenAsync();

            var page = await _restService.GetAsync<HateoasPage<GameInfo>>(url, authToken);
            _nextUri = page.GetLink("next")?.OriginalString;

            var result = new List<GameInfo>();
            // If the page returns doesn't contain any data, then it'll be null and not returned in the response.
            if (page.Embedded?.Data != null)
            {
                result.AddRange(page.Embedded?.Data);
            }

            return result;
        }

        /// <summary>
        /// Utility method used to convert a <see cref="GameInfo"/> instance into a <see cref="ListItemViewModel"/>.
        /// This is done as the list views used by the view accept the values mapped within these view models for
        /// code re-use purposes.
        /// </summary>
        /// <param name="gameInfo">The <see cref="GameInfo"/> object to convert.</param>
        /// <returns>A <see cref="ListItemViewModel"/> with the data retrieved from the <see cref="GameInfo"/> instance.</returns>
        private ListItemViewModel CreateListItemViewModelFromGameInfo(GameInfo gameInfo)
        {
            return new ListItemViewModel
            {
                ImageUrl = gameInfo.GetLink("image"),
                Header = string.Join(", ", gameInfo.Platforms),
                ItemTitle = gameInfo.Title,
                ItemSubTitle =
                    $"{gameInfo.ReleaseDate:MMMM yyyy}, {string.Join(", ", gameInfo.Publishers)}",
                ShowRating = false,
                TapCommand = new DelegateCommand(async () =>
                {
                    // We only need to include a small number of parameters, as the game isn't
                    // classed as being in our collection at this point as it can't be narrowed
                    // down to a single console.
                    var parameters = new NavigationParameters
                    {
                        {"game-url", gameInfo.GetLink("self")},
                        {"in-library", false}
                    };

                    await NavigationService.NavigateAsync("GamePage", parameters);
                })
            };
        }
    }
}