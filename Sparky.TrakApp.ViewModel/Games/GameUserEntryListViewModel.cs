﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Common.Extensions;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;
using Xamarin.Essentials;

namespace Sparky.TrakApp.ViewModel.Games
{
    public abstract class GameUserEntryListViewModel : BaseViewModel, IActiveAware
    {
        private readonly IStorageService _storageService;
        private readonly IUserDialogs _userDialogs;
        private readonly IRestService _restService;

        private bool _isActive;
        
        private bool _hasNext = true;
        private Uri _nextPageUri;
        private bool _isEmpty;

        private ObservableCollection<GameUserEntry> _gameUserEntries;
        
        protected GameUserEntryListViewModel(INavigationService navigationService, IStorageService storageService, IUserDialogs userDialogs, IRestService restService, GameUserEntryStatus gameUserEntryStatus) : base(navigationService)
        {
            _restService = restService;
            _storageService = storageService;
            _userDialogs = userDialogs;
            
            GameUserEntryStatus = gameUserEntryStatus;
            GameUserEntries = new ObservableCollection<GameUserEntry>();
            
            LoadGameUserEntriesCommand = new DelegateCommand(async () => await LoadUserGameEntriesOnActiveAsync(), () => !IsBusy);
            LoadGameUserEntriesNextPageCommand = new DelegateCommand(async () => await LoadGameUserEntriesNextPageAsync(), () => !IsBusy && _hasNext);
        }

        public ICommand LoadGameUserEntriesCommand { get; set; }
        
        public ICommand LoadGameUserEntriesNextPageCommand { get; set; }
        
        public GameUserEntryStatus GameUserEntryStatus { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, async () => await LoadUserGameEntriesOnActiveAsync());
        }

        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }
        
        public ObservableCollection<GameUserEntry> GameUserEntries
        {
            get => _gameUserEntries;
            set => SetProperty(ref _gameUserEntries, value);
        }

        private async Task LoadGameUserEntriesNextPageAsync()
        {
            IsError = false;
            IsBusy = true;

            try
            {
                // Load the next page of game user entries with the next page URL.
                var result = await GetGameUserEntriesPageAsync(_nextPageUri.OriginalString);
                foreach (var gameUserEntry in result)
                {
                    GameUserEntries.Add(gameUserEntry);
                }
            }
            catch (ApiException e)
            {
                IsError = true;
                _userDialogs.Toast(new ToastConfig(Messages.GameUserEntryListPageEmptyServerError)
                    .SetBackgroundColor(ColorConverters.FromHex("#FF0000"))
                    .SetMessageTextColor(Color.White)
                    .SetDuration(TimeSpan.FromSeconds(5))
                    .SetPosition(ToastPosition.Bottom));
            }
            catch (Exception e)
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

        private async Task LoadUserGameEntriesOnActiveAsync()
        {
            // If the tab is not being navigated to, then there's no point making a server request.
            if (!_isActive)
            {
                // Clear the current list when navigating away so it can be freshly reloaded when navigating back.
                GameUserEntries.Clear();
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
                
                GameUserEntries = new ObservableCollection<GameUserEntry>(
                    await GetGameUserEntriesPageAsync($"api/game-management/v1/game-user-entries?user-id={userId}&status={enumName}&sort=gameTitle"));

                IsEmpty = GameUserEntries.Count == 0;
            }
            catch (ApiException e)
            {
                IsError = true;
                ErrorMessage = Messages.GameUserEntryListPageEmptyServerError;
            }
            catch (Exception e)
            {
                IsError = true;
                ErrorMessage = Messages.GameUserEntryListPageEmptyGenericError;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<IEnumerable<GameUserEntry>> GetGameUserEntriesPageAsync(string url)
        {
            // Get the needed variables from the store.
            var authToken = await _storageService.GetAuthTokenAsync();
            
            var page = await _restService.GetAsync<HateoasPage<GameUserEntry>>(url, authToken);

            _hasNext = page.HasNext;
            _nextPageUri = _hasNext ? page.Links["next"].Href : null;

            var result = new List<GameUserEntry>();
            // If the page returns doesn't contain any data, then it'll be null and not returned in the response.
            if (page.Embedded?.Data != null)
            {
                result.AddRange(page.Embedded?.Data);
            }
            
            return result;
        }
        
        public event EventHandler IsActiveChanged;
    }
}