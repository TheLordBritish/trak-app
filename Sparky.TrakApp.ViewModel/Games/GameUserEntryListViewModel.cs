using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.AppCenter.Crashes;
using Prism;
using Prism.Commands;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sparky.TrakApp.Common;
using Sparky.TrakApp.Common.Extensions;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Common;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameLibraryListViewModel"/> is the view model that is associated with a game user entry list page view.
    /// Its responsibility is to display the users personal collection of games within each of the different <see cref="GameUserEntryStatus"/>
    /// categories. It also allows for the user to add games into their collection from the user agnostic game library.
    /// </summary>
    public abstract class GameUserEntryListViewModel : BaseListViewModel<GameUserEntry, ListItemViewModel>, IActiveAware
    {
        private readonly IRestService _restService;
        private readonly IStorageService _storageService;

        private readonly GameUserEntryStatus _gameUserEntryStatus;
        private string _nextUri;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="userDialogs">The <see cref="IUserDialogs"/> instance to inject.</param>
        /// <param name="gameUserEntryStatus">The <see cref="GameUserEntryStatus"/> that the list reflects user entries on.</param>
        protected GameUserEntryListViewModel(IScheduler scheduler, INavigationService navigationService,
            IStorageService storageService,
            IUserDialogs userDialogs, IRestService restService, GameUserEntryStatus gameUserEntryStatus) : base(
            scheduler, navigationService)
        {
            _restService = restService;
            _storageService = storageService;

            _gameUserEntryStatus = gameUserEntryStatus;

            LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync, outputScheduler: scheduler);
            // Register to the result of the search command and convert the result into list item view models.
            LoadCommand.Subscribe(results =>
            {
                IsError = false;

                Items.Clear();
                Items.AddRange(results.Select(CreateListItemViewModelFromGameUserEntry));

                IsEmpty = Items.Count == 0;
            });
            // Report errors if an exception was thrown.
            LoadCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (ex is ApiException)
                {
                    ErrorMessage = Messages.GameLibraryListPageEmptyServerError;
                }
                else
                {
                    ErrorMessage = Messages.GameLibraryListPageEmptyGenericError;
                    Crashes.TrackError(ex);
                }
            });

            var canLoadMore = this.WhenAnyValue(x => x._nextUri, x => !string.IsNullOrEmpty(x));

            LoadMoreCommand =
                ReactiveCommand.CreateFromTask<Unit, IEnumerable<GameUserEntry>>(
                    _ => LoadFromUrlAsync(_nextUri),
                    canLoadMore, scheduler);
            // Register to the result of the load more command and convert the result into list item view models.
            LoadMoreCommand.Subscribe(results =>
            {
                IsError = false;
                Items.AddRange(results.Select(CreateListItemViewModelFromGameUserEntry));
            });
            // Report errors if an exception was thrown.
            LoadMoreCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (ex is ApiException)
                {
                    userDialogs.Toast(new ToastConfig(Messages.GameUserEntryListPageEmptyServerError)
                        .SetBackgroundColor(Color.Red)
                        .SetMessageTextColor(Color.White)
                        .SetDuration(TimeSpan.FromSeconds(5))
                        .SetPosition(ToastPosition.Bottom));
                }
                else
                {
                    userDialogs.Toast(new ToastConfig(Messages.GameUserEntryListPageEmptyGenericError)
                        .SetBackgroundColor(Color.Red)
                        .SetMessageTextColor(Color.White)
                        .SetDuration(TimeSpan.FromSeconds(5))
                        .SetPosition(ToastPosition.Bottom));
                    
                    Crashes.TrackError(ex);
                }
            });

            AddGameCommand = ReactiveCommand.CreateFromTask(AddGameAsync, outputScheduler: scheduler);

            this.WhenAnyObservable(x => x.LoadCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);

            this.WhenAnyObservable(x => x.LoadMoreCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsRefreshing, scheduler: scheduler);

            this.WhenAnyValue(x => x.IsActive)
                .Where(x => x)
                .Select(x => Unit.Default)
                .InvokeCommand(LoadCommand);
        }

        /// <summary>
        /// Whether the current <see cref="GameUserEntryListViewModel"/> instance is the active tab within
        /// the tab page.
        /// </summary>
        [Reactive]
        public bool IsActive { get; set; }

        // Disabled as not used but has to be declared to implement IActiveAware.
#pragma warning disable 067
        public event EventHandler IsActiveChanged;
#pragma warning restore 067

        /// <summary>
        /// Command that is invoked each the time the add game button is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="AddGameAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddGameCommand { get; }

        /// <summary>
        /// Private method that is invoked by the <see cref="AddGameCommand"/>. When invoked, it will navigate the user
        /// to the game library tabbed pages which allow the user to add a game either through searching or scanning
        /// a barcode.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AddGameAsync()
        {
            await NavigationService.NavigateAsync(
                "GameLibraryTabbedPage?createTab=GameLibraryListPage&createTab=GameBarcodeScannerPage",
                new NavigationParameters
                {
                    {"transition-type", TransitionType.SlideFromBottom}
                });
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoadCommand"/>. When invoked, it will first check that the view model
        /// is the currently selected tab before creating the initial URI query and calling off to the API to retrieve the first
        /// page of <see cref="GameUserEntry"/> instances for the logged in user.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="GameUserEntry"/> instances which is the first page for the logged in user.
        /// </returns>
        private async Task<IEnumerable<GameUserEntry>> LoadAsync()
        {
            if (!IsActive)
            {
                return Enumerable.Empty<GameUserEntry>();
            }

            // Get the ID of the user currently logged in.
            var userId = await _storageService.GetUserIdAsync();
            // Get the name of the status for the currently selected tab.
            var status = _gameUserEntryStatus.GetAttributeValue<EnumMemberAttribute, string>(s => s.Value);

            // Make the initial request to load the first page.
            _nextUri = $"api/game-management/v1/game-user-entries?user-id={userId}&status={status}&sort=gameTitle";
            return await LoadFromUrlAsync(_nextUri);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoadMoreCommand"/> command and the <see cref="LoadAsync"/> method.
        /// When invoked, it will call off to the API and retrieve a page of <see cref="GameUserEntry"/>'s for the given user.
        /// If the page returned contains more information in subsequent pages, the next page URI is stored for when the next
        /// page needs to be loaded by the view.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="GameUserEntry"/> instances which is the specified page for the logged in user.
        /// </returns>
        private async Task<IEnumerable<GameUserEntry>> LoadFromUrlAsync(string url)
        {
            // Get the needed variables from the store.
            var authToken = await _storageService.GetAuthTokenAsync();

            var page = await _restService.GetAsync<HateoasPage<GameUserEntry>>(url, authToken);

            _nextUri = page.GetLink("next")?.OriginalString;

            var result = new List<GameUserEntry>();
            // If the page returns doesn't contain any data, then it'll be null and not returned in the response.
            if (page.Embedded?.Data != null)
            {
                result.AddRange(page.Embedded?.Data);
            }

            return result;
        }

        /// <summary>
        /// Utility method used to convert a <see cref="GameUserEntry"/> instance into a <see cref="ListItemViewModel"/>.
        /// This is done as the list views used by the view accept the values mapped within these view models for
        /// code re-use purposes.
        /// </summary>
        /// <param name="gameUserEntry">The <see cref="GameUserEntry"/> object to convert.</param>
        /// <returns>A <see cref="ListItemViewModel"/> with the data retrieved from the <see cref="GameUserEntry"/> instance.</returns>
        private ListItemViewModel CreateListItemViewModelFromGameUserEntry(GameUserEntry gameUserEntry)
        {
            return new ListItemViewModel
            {
                ImageUrl = gameUserEntry.GetLink("image"),
                Header = gameUserEntry.PlatformName,
                ItemTitle = gameUserEntry.GameTitle,
                ItemSubTitle =
                    $"{gameUserEntry.GameReleaseDate:MMMM yyyy}, {string.Join(", ", gameUserEntry.Publishers)}",
                ShowRating = true,
                Rating = gameUserEntry.Rating,
                TapCommand = new DelegateCommand(async () =>
                {
                    var parameters = new NavigationParameters
                    {
                        {"game-url", gameUserEntry.GetLink("gameInfo")},
                        {"platform-id", gameUserEntry.PlatformId},
                        {"in-library", true},
                        {"rating", gameUserEntry.Rating},
                        {"status", gameUserEntry.Status}
                    };

                    await NavigationService.NavigateAsync("GamePage", parameters);
                })
            };
        }
    }
}