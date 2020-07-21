using System;
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
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Games
{
    public class GameOptionsViewModel : BaseViewModel
    {
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;
        
        private Uri _gameUrl;
        private long _gameId;
        private long _platformId;
        private bool _inLibrary = true;
        private bool _isUpdated;

        private GameUserEntryStatus _originalStatus;
        private ObservableCollection<GameUserEntryStatus> _statuses;
        private GameUserEntryStatus _selectedStatus;
        
        public GameOptionsViewModel(INavigationService navigationService, IStorageService storageService, IRestService restService) : base(navigationService)
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
        }
        
        public ICommand UpdateGameCommand => new DelegateCommand(async () => await UpdateGameAsync());
        
        public ICommand DeleteGameCommand => new DelegateCommand(async () => await DeleteGameAsync());
        
        public ObservableCollection<GameUserEntryStatus> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        public GameUserEntryStatus SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }
        
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            parameters.Add("game-url", _gameUrl);
            parameters.Add("platform-id", _platformId);
            parameters.Add("in-library", _inLibrary);
            parameters.Add("status", _inLibrary && _isUpdated ? SelectedStatus : _originalStatus);
        }
        
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _gameUrl = parameters.GetValue<Uri>("game-url");
            _gameId = parameters.GetValue<long>("game-id");
            _platformId = parameters.GetValue<long>("platform-id");
            _originalStatus = parameters.GetValue<GameUserEntryStatus>("status");
            SelectedStatus = _originalStatus;
        }

        private async Task UpdateGameAsync()
        {
            IsError = false;
            IsBusy = true;

            try
            {
                // Get the needed values to make the call from the storage service.
                var userId = await _storageService.GetUserIdAsync();
                var token = await _storageService.GetAuthTokenAsync();

                // Make the request to see if the game they're adding is already in their library.
                var existingEntry =
                    await _restService.GetAsync<HateoasPage<GameUserEntry>>($"api/game-management/v1/game-user-entries?user-id={userId}&platform-id={_platformId}&game-id={_gameId}",
                        token);

                if (existingEntry.Embedded != null)
                {
                    // If the entry is already in the users library, just update it with the selected status they provided.
                    var entry = existingEntry.Embedded.Data.First();
                    entry.Status = SelectedStatus;
                    
                    // Make a request to update the game to their collection.
                    await _restService.PutAsync("api/game-management/v1/game-user-entries", entry, token);
                }

                _isUpdated = true;
                await NavigationService.GoBackAsync();
            }
            catch (ApiException)
            {
                IsError = true;
                ErrorMessage = Messages.ErrorMessageApiError;
            }
            catch (Exception)
            {
                IsError = true;
                ErrorMessage = Messages.ErrorMessageGeneric;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteGameAsync()
        {
            IsError = false;
            IsBusy = true;

            try
            {
                // Get the needed values to make the call from the storage service.
                var userId = await _storageService.GetUserIdAsync();
                var token = await _storageService.GetAuthTokenAsync();

                // Make the request to see if the game they're adding is already in their library.
                var existingEntry =
                    await _restService.GetAsync<HateoasPage<GameUserEntry>>($"api/game-management/v1/game-user-entries?user-id={userId}&platform-id={_platformId}&game-id={_gameId}",
                        token);

                if (existingEntry.Embedded != null)
                {
                    // Make a request to delete the game from their collection.
                    await _restService.DeleteAsync($"api/game-management/v1/game-user-entries/{existingEntry.Embedded.Data.First().Id}", token);
                }

                _inLibrary = false;
                await NavigationService.GoBackAsync();
            }
            catch (ApiException)
            {
                IsError = true;
                ErrorMessage = Messages.ErrorMessageApiError;
            }
            catch (Exception)
            {
                IsError = true;
                ErrorMessage = Messages.ErrorMessageGeneric;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}