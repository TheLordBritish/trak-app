using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Prism;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Common.Extensions;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Common;
using Sparky.TrakApp.ViewModel.Resources;
using Xamarin.Essentials;

namespace Sparky.TrakApp.ViewModel.Games
{
    public abstract class GameUserEntryListViewModel : BaseListViewModel<ListItemViewModel>, IActiveAware
    {
        private readonly IRestService _restService;
        private readonly IStorageService _storageService;
        private readonly IUserDialogs _userDialogs;

        private bool _hasNext = true;

        private bool _isActive;
        private Uri _nextPageUri;

        protected GameUserEntryListViewModel(INavigationService navigationService, IStorageService storageService,
            IUserDialogs userDialogs, IRestService restService, GameUserEntryStatus gameUserEntryStatus) : base(
            navigationService)
        {
            _restService = restService;
            _storageService = storageService;
            _userDialogs = userDialogs;

            GameUserEntryStatus = gameUserEntryStatus;

            // Commands
            RefreshCommand = new DelegateCommand(async () => await LoadUserGameEntriesAsync(), () => !IsBusy);
            LoadMoreCommand = new DelegateCommand(async () => await LoadGameUserEntriesNextPageAsync(),
                () => !IsBusy && _hasNext);
        }

        public GameUserEntryStatus GameUserEntryStatus { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, async () => await LoadUserGameEntriesAsync());
        }

        public event EventHandler IsActiveChanged;

        private async Task LoadGameUserEntriesNextPageAsync()
        {
            IsError = false;
            IsBusy = true;

            try
            {
                // Load the next page of game user entries with the next page URL.
                var result = await GetGameUserEntriesAsync(_nextPageUri.OriginalString);
                foreach (var gameUserEntry in result)
                {
                    Items.Add(CreateListItemViewModelFromGameUserEntry(gameUserEntry));
                }
            }
            catch (ApiException)
            {
                IsError = true;
                _userDialogs.Toast(new ToastConfig(Messages.GameUserEntryListPageEmptyServerError)
                    .SetBackgroundColor(ColorConverters.FromHex("#FF0000"))
                    .SetMessageTextColor(Color.White)
                    .SetDuration(TimeSpan.FromSeconds(5))
                    .SetPosition(ToastPosition.Bottom));
            }
            catch (Exception)
            {
                IsError = true;
                _userDialogs.Toast(new ToastConfig(Messages.GameUserEntryListPageEmptyGenericError)
                    .SetBackgroundColor(ColorConverters.FromHex("#FF0000"))
                    .SetMessageTextColor(Color.White)
                    .SetDuration(TimeSpan.FromSeconds(5))
                    .SetPosition(ToastPosition.Bottom));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadUserGameEntriesAsync()
        {
            // If the tab is not being navigated to, then there's no point making a server request.
            if (!_isActive)
            {
                // Clear the current list when navigating away so it can be freshly reloaded when navigating back.
                Items.Clear();
                return;
            }

            IsError = false;
            IsBusy = true;

            try
            {
                // Get the ID of the user currently logged in.
                var userId = await _storageService.GetUserIdAsync();
                // Make the initial request to load the first page.
                var enumName = GameUserEntryStatus.GetAttributeValue<EnumMemberAttribute, string>(s => s.Value);

                var entries = await GetGameUserEntriesAsync(
                    $"api/game-management/v1/game-user-entries?user-id={userId}&status={enumName}&sort=gameTitle");

                Items = new ObservableCollection<ListItemViewModel>(entries.Select(CreateListItemViewModelFromGameUserEntry));
                
                IsEmpty = Items.Count == 0;
            }
            catch (ApiException)
            {
                IsError = true;
                ErrorMessage = Messages.GameUserEntryListPageEmptyServerError;
            }
            catch (Exception)
            {
                IsError = true;
                ErrorMessage = Messages.GameUserEntryListPageEmptyGenericError;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<IEnumerable<GameUserEntry>> GetGameUserEntriesAsync(string url)
        {
            // Get the needed variables from the store.
            var authToken = await _storageService.GetAuthTokenAsync();

            var page = await _restService.GetAsync<HateoasPage<GameUserEntry>>(url, authToken);

            _hasNext = page.HasNext;
            _nextPageUri = _hasNext ? page.GetLink("next") : null;

            var result = new List<GameUserEntry>();
            // If the page returns doesn't contain any data, then it'll be null and not returned in the response.
            if (page.Embedded?.Data != null)
            {
                result.AddRange(page.Embedded?.Data);
            }

            return result;
        }

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
                        {"platform-url", gameUserEntry.GetLink("platform")},
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