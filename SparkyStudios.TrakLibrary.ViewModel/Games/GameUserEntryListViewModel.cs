using System;
using System.Collections.Generic;
using System.Drawing;
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
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Games
{
    public class GameUserEntryListViewModel : BaseListViewModel<GameUserEntry, ListItemViewModel>
    {
        private readonly IRestService _restService;

        private string _firstUri;
        private string _nextUri;
        
        public GameUserEntryListViewModel(IScheduler scheduler, INavigationService navigationService, IRestService restService, IUserDialogs userDialogs) : base(scheduler, navigationService)
        {
            _restService = restService;
            
            AddGameCommand = ReactiveCommand.CreateFromTask(AddGameAsync, outputScheduler: scheduler);
            
            LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync, outputScheduler: scheduler);
            // Register to the result of the search command and convert the result into list item view models.
            LoadCommand.Subscribe(results =>
            {
                IsError = false;

                Items.Clear();
                Items.AddRange(results.Select(CreateListItemViewModelFromGameUserEntry));
                
                HasLoaded = true;
            });
            // Report errors if an exception was thrown.
            LoadCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                HasLoaded = true;
                
                Items.Clear();

                switch (ex)
                {
                    case TaskCanceledException _:
                        ErrorMessage = Messages.ErrorMessageNoInternet;
                        break;
                    case ApiException _:
                        ErrorMessage = Messages.GameLibraryListPageEmptyServerError;
                        break;
                    default:
                        ErrorMessage = Messages.GameLibraryListPageEmptyGenericError;
                        Crashes.TrackError(ex);
                        break;
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
                switch (ex)
                {
                    case TaskCanceledException _:
                        userDialogs.Toast(new ToastConfig(Messages.ErrorMessageNoInternet)
                            .SetBackgroundColor(Color.Red)
                            .SetMessageTextColor(Color.White)
                            .SetDuration(TimeSpan.FromSeconds(5))
                            .SetPosition(ToastPosition.Bottom));
                        break;
                    case ApiException _:
                        userDialogs.Toast(new ToastConfig(Messages.GameUserEntryListPageEmptyServerError)
                            .SetBackgroundColor(Color.Red)
                            .SetMessageTextColor(Color.White)
                            .SetDuration(TimeSpan.FromSeconds(5))
                            .SetPosition(ToastPosition.Bottom));
                        break;
                    default:
                        userDialogs.Toast(new ToastConfig(Messages.GameUserEntryListPageEmptyGenericError)
                            .SetBackgroundColor(Color.Red)
                            .SetMessageTextColor(Color.White)
                            .SetDuration(TimeSpan.FromSeconds(5))
                            .SetPosition(ToastPosition.Bottom));
                    
                        Crashes.TrackError(ex);
                        break;
                }
            });
            
            this.WhenAnyObservable(x => x.LoadCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.New)
            {
                _firstUri = parameters.GetValue<string>("base-url");
                LoadCommand.Execute()
                    .Catch(Observable.Return(Enumerable.Empty<GameUserEntry>()))
                    .Subscribe();
            }
        }
        
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
                "GameLibraryTabbedPage?createTab=GameLibraryListPage&createTab=GameBarcodeScannerPage");
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
            HasLoaded = false;
            // Make the initial request to load the first page.
            return await LoadFromUrlAsync(_firstUri);
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
            var page = await _restService.GetAsync<HateoasPage<GameUserEntry>>(url);

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
                ImageUrl = gameUserEntry.GetLink("small_image"),
                HeaderDetails = gameUserEntry.GameUserEntryPlatforms?.Select(x => new ItemEntryViewModel
                {
                    Name = x.PlatformName,
                    IsSelected = true
                }).OrderBy(x => x.Name).ToList(),
                ItemTitle = gameUserEntry.GameTitle,
                ItemSubTitle = string.Join(", ", gameUserEntry.Publishers),
                ShowRating = true,
                Rating = gameUserEntry.Rating,
                TapCommand = new DelegateCommand(async () =>
                {
                    var parameters = new NavigationParameters
                    {
                        {"game-url", gameUserEntry.GetLink("game_details")}
                    };

                    await NavigationService.NavigateAsync("GamePage", parameters);
                })
            };
        }
    }
}