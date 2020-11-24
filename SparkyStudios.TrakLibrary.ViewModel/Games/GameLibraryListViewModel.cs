using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.AppCenter.Crashes;
using Prism.Commands;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkyStudios.TrakLibrary.Common;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using Color = System.Drawing.Color;

namespace SparkyStudios.TrakLibrary.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameLibraryListViewModel"/> is the view model that is associated with the game library list page view.
    /// Its responsibility is to respond to user defined search queries and display a list of <see cref="GameDetails"/> that
    /// match the given criteria. The user can click on entries from here to add them to their own library.
    ///
    /// If the user searches for a game that isn't in the library, this view model also allows them to make requests
    /// for the game to be added to the library.
    /// </summary>
    public class GameLibraryListViewModel : ReactiveViewModel
    {
        private readonly IRestService _restService;

        private string _nextUri;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        /// <param name="userDialogs">The <see cref="IUserDialogs"/> instance to inject.</param>
        public GameLibraryListViewModel(IScheduler scheduler, INavigationService navigationService,
            IRestService restService, IUserDialogs userDialogs) : base(scheduler, navigationService)
        {
            _restService = restService;

            var canSearch = this.WhenAny(x => x.SearchQuery, x => !string.IsNullOrWhiteSpace(x.Value));

            SearchCommand =
                ReactiveCommand.CreateFromTask<string, IEnumerable<GameDetails>>(SearchAsync, canSearch, scheduler);
            // Register to the result of the search command and convert the result into list item view models.
            SearchCommand.Subscribe(results =>
            {
                Items.Clear();
                Items.AddRange(results.Select(CreateListItemViewModelFromGameInfo));

                if (Items.Count == 0)
                {
                    Message = string.Format(Messages.GameLibraryPageEmptySearchResult, SearchQuery);
                }
            });
            // Report errors if an exception was thrown.
            SearchCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (ex is ApiException)
                {
                    Message = Messages.GameLibraryListPageEmptyServerError;
                }
                else
                {
                    Message = Messages.GameLibraryListPageEmptyGenericError;
                    Crashes.TrackError(ex, new Dictionary<string, string>
                    {
                        {"Search query", SearchQuery}
                    });
                }
            });

            var canLoadMore = this.WhenAnyValue(x => x._nextUri, x => !string.IsNullOrEmpty(x));

            LoadMoreCommand =
                ReactiveCommand.CreateFromTask<Unit, IEnumerable<GameDetails>>(_ => GetGamesFromUrlAsync(_nextUri),
                    canLoadMore, scheduler);
            // Register to the result of the load more command and convert the result into list item view models.
            LoadMoreCommand.Subscribe(results =>
            {
                Items.AddRange(results.Select(CreateListItemViewModelFromGameInfo));
            });
            // Report errors if an exception was thrown.
            LoadMoreCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (ex is ApiException)
                {
                    userDialogs.Toast(new ToastConfig(Messages.GameLibraryListPageEmptyServerError)
                        .SetBackgroundColor(Color.Red)
                        .SetMessageTextColor(Color.White)
                        .SetDuration(TimeSpan.FromSeconds(5))
                        .SetPosition(ToastPosition.Bottom));
                }
                else
                {
                    userDialogs.Toast(new ToastConfig(Messages.GameLibraryListPageEmptyGenericError)
                        .SetBackgroundColor(Color.Red)
                        .SetMessageTextColor(Color.White)
                        .SetDuration(TimeSpan.FromSeconds(5))
                        .SetPosition(ToastPosition.Bottom));
                    
                    Crashes.TrackError(ex, new Dictionary<string, string>
                    {
                        {"Search query", SearchQuery}
                    });
                }
            });

            this.WhenAnyValue(x => x.SearchQuery)
                .ObserveOn(scheduler)
                .Where(string.IsNullOrEmpty)
                .Subscribe(x => Message = Messages.GameLibraryListPageInstructions);
            
            this.WhenAnyObservable(x => x.SearchCommand.IsExecuting, x => x.LoadMoreCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);

            this.WhenAnyObservable(x => x.SearchCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsSearching, scheduler: scheduler);
            
            RequestCommand = ReactiveCommand.CreateFromTask(RequestAsync, outputScheduler: scheduler);
        }
        
        /// <summary>
        /// An <see cref="ObservableRangeCollection{T}"/> that contains all of the <see cref="ListItemViewModel"/>
        /// items that each represent a separate <see cref="GameDetails"/>.
        /// </summary>
        [Reactive]
        public ObservableRangeCollection<ListItemViewModel> Items { get; private set; } =
            new ObservableRangeCollection<ListItemViewModel>();

        /// <summary>
        /// A <see cref="string"/> that represents the current message to display to the user if there are empty
        /// results. 
        /// </summary>
        [Reactive] 
        public string Message { get; set; } = Messages.GameLibraryListPageInstructions;
        
        /// <summary>
        /// A <see cref="string"/> that contains the currently populated user defined query to search
        /// the game library with.
        /// </summary>
        [Reactive]
        public string SearchQuery { get; set; }

        /// <summary>
        /// A readonly <see cref="bool"/> property that designates whether a query is currently being searched.
        /// </summary>
        public bool IsSearching { [ObservableAsProperty] get; }

        /// <summary>
        /// Command that is invoked each the time the search query is changed by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="SearchAsync"/> method.
        /// </summary>
        public ReactiveCommand<string, IEnumerable<GameDetails>> SearchCommand { get; }

        /// <summary>
        /// Command that is automatically invoked each the time the user scrolls to the bottom of the list and more
        /// items can be loaded in from the serve. When called, the command will propagate the request and call the
        /// <see cref="GetGamesFromUrlAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, IEnumerable<GameDetails>> LoadMoreCommand { get; }

        /// <summary>
        /// Command that is invoked each the time the make a request button is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="RequestAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RequestCommand { get; }

        /// <summary>
        /// Private method that is invoked by the <see cref="SearchCommand"/> when activated by the associated
        /// view. This method will set the URI to call the <see cref="GameDetails"/> data from and return an
        /// <see cref="IEnumerable{T}"/> of all games associated with the given query.
        /// </summary>
        /// <returns>A <see cref="Task"/> which contains all of the <see cref="GameDetails"/> objects associated with the query.</returns>
        private async Task<IEnumerable<GameDetails>> SearchAsync(string query)
        {
            _nextUri = $"games/details?title={query}&sort=title";
            return await GetGamesFromUrlAsync(_nextUri);
        }
        
        /// <summary>
        /// Private method that is invoked by the <see cref="RequestCommand"/> when activated by the associated
        /// view. This method will navigate the user to the game request page.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task RequestAsync()
        {
            await NavigationService.NavigateAsync("GameRequestPage");
        }

        /// <summary>
        /// Retrieves an <see cref="IEnumerable{T}"/> of <see cref="GameDetails"/> objects from the server by
        /// invoking the given url. No error checking is done within this method, it is done by the commands
        /// that invoke it.
        ///
        /// If the data loaded has an additional page, the next URI is set and more data can be loaded when
        /// the user reaches the end of the collection.
        /// </summary>
        /// <param name="url">The url to invoke to retrieve another page of <see cref="GameDetails"/> instances from.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="GameDetails"/> objects from the given page.</returns>
        private async Task<IEnumerable<GameDetails>> GetGamesFromUrlAsync(string url)
        {
            var page = await _restService.GetAsync<HateoasPage<GameDetails>>(url);
            _nextUri = page.GetLink("next")?.OriginalString;

            var result = new List<GameDetails>();
            // If the page returns doesn't contain any data, then it'll be null and not returned in the response.
            if (page.Embedded?.Data != null)
            {
                result.AddRange(page.Embedded?.Data);
            }

            return result;
        }

        /// <summary>
        /// Utility method used to convert a <see cref="GameDetails"/> instance into a <see cref="ListItemViewModel"/>.
        /// This is done as the list views used by the view accept the values mapped within these view models for
        /// code re-use purposes.
        /// </summary>
        /// <param name="gameDetails">The <see cref="GameDetails"/> object to convert.</param>
        /// <returns>A <see cref="ListItemViewModel"/> with the data retrieved from the <see cref="GameDetails"/> instance.</returns>
        private ListItemViewModel CreateListItemViewModelFromGameInfo(GameDetails gameDetails)
        {
            var platforms = gameDetails.Platforms?.Select(x => new ItemEntryViewModel
            {
                Name = x.Name,
                IsSelected = true
            }).ToList();

            return new ListItemViewModel
            {
                ImageUrl = gameDetails.GetLink("image"),
                HeaderDetails = platforms,
                ItemTitle = gameDetails.Title,
                ItemSubTitle = string.Join(", ", gameDetails.Publishers.Select(x => x.Name)),
                ShowRating = false,
                TapCommand = new DelegateCommand(async () =>
                {
                    // We only need to include a small number of parameters, as the game isn't
                    // classed as being in our collection at this point as it can't be narrowed
                    // down to a single console.
                    var parameters = new NavigationParameters
                    {
                        {"game-url", gameDetails.GetLink("self")}
                    };

                    await NavigationService.NavigateAsync("GamePage", parameters, false, false);
                })
            };
        }
    }
}