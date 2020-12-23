using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkyStudios.TrakLibrary.Common.Extensions;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;

namespace SparkyStudios.TrakLibrary.ViewModel.Games
{
    public class GameUserEntryFilterViewModel : ReactiveViewModel
    {
        private readonly IRestService _restService;
        
        public GameUserEntryFilterViewModel(IScheduler scheduler, INavigationService navigationService, IRestService restService) : base(scheduler, navigationService)
        {
            _restService = restService;
            
            LoadGameFiltersCommand = ReactiveCommand.CreateFromTask(LoadGameFiltersAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            LoadGameFiltersCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (!(ex is ApiException))
                {
                    Crashes.TrackError(ex);
                }
            });
            
            PlatformGroupTappedCommand = ReactiveCommand.Create<bool>(b =>
            {
                IsPlatformGroupExpanded = !IsPlatformGroupExpanded;
            });
            GenreGroupTappedCommand = ReactiveCommand.Create<bool>(b =>
            {
                IsGenreGroupExpanded = !IsGenreGroupExpanded;
            });
            GameModeGroupTappedCommand = ReactiveCommand.Create<bool>(b =>
            {
                IsGameModeGroupExpanded = !IsGameModeGroupExpanded;
            });
            AgeRatingGroupTappedCommand = ReactiveCommand.Create<bool>(b =>
            {
                IsAgeRatingGroupExpanded = !IsAgeRatingGroupExpanded;
            });
            StatusGroupTappedCommand = ReactiveCommand.Create<bool>(b =>
            {
                IsStatusGroupExpanded = !IsStatusGroupExpanded;
            });
            
            PlatformTappedCommand = ReactiveCommand.Create<ItemEntryViewModel>(item =>
            {
                if (item == null) return;
                
                var index = Platforms.IndexOf(item);
                    
                item.IsSelected = !item.IsSelected;
                Platforms[index] = item;
            });
            
            GenreTappedCommand = ReactiveCommand.Create<ItemEntryViewModel>(item =>
            {
                if (item == null) return;
                
                var index = Genres.IndexOf(item);
                    
                item.IsSelected = !item.IsSelected;
                Genres[index] = item;
            });
            
            GameModeTappedCommand = ReactiveCommand.Create<ItemEntryViewModel>(item =>
            {
                if (item == null) return;
                
                var index = GameModes.IndexOf(item);
                    
                item.IsSelected = !item.IsSelected;
                GameModes[index] = item;
            });
            
            AgeRatingTappedCommand = ReactiveCommand.Create<ItemEntryViewModel>(item =>
            {
                if (item == null) return;
                
                var index = AgeRatings.IndexOf(item);
                    
                item.IsSelected = !item.IsSelected;
                AgeRatings[index] = item;
            });
            
            StatusTappedCommand = ReactiveCommand.Create<ItemEntryViewModel>(item =>
            {
                if (item == null) return;
                
                var index = Statuses.IndexOf(item);
                    
                item.IsSelected = !item.IsSelected;
                Statuses[index] = item;
            });
            
            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyAsync, outputScheduler: scheduler);

            this.WhenAnyObservable(x => x.LoadGameFiltersCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);
        }
        
        /// <summary>
        /// A <see cref="bool"/> that specifies whether the platform filters have been expanded on the view.
        /// </summary>
        [Reactive]
        public bool IsPlatformGroupExpanded { get; set; }
        
        /// <summary>
        /// A <see cref="bool"/> that specifies whether the genre filters have been expanded on the view.
        /// </summary>
        [Reactive]
        public bool IsGenreGroupExpanded { get; set; }
        
        /// <summary>
        /// A <see cref="bool"/> that specifies whether the game mode filters have been expanded on the view.
        /// </summary>
        [Reactive]
        public bool IsGameModeGroupExpanded { get; set; }
        
        /// <summary>
        /// A <see cref="bool"/> that specifies whether the age rating filters have been expanded on the view.
        /// </summary>
        [Reactive]
        public bool IsAgeRatingGroupExpanded { get; set; }

        /// <summary>
        /// A <see cref="bool"/> that specifies whether the statuses filters have been expanded on the view.
        /// </summary>
        [Reactive]
        public bool IsStatusGroupExpanded { get; set; }

        /// <summary>
        /// A <see cref="ObservableCollection{T}" /> that contains all the selectable platform filters.
        /// </summary>
        [Reactive]
        public ObservableCollection<ItemEntryViewModel> Platforms { get; set; }
        
        /// <summary>
        /// A <see cref="ObservableCollection{T}" /> that contains all the selectable genre filters.
        /// </summary>
        [Reactive]
        public ObservableCollection<ItemEntryViewModel> Genres { get; set; }
        
        /// <summary>
        /// A <see cref="ObservableCollection{T}" /> that contains all the selectable game mode filters.
        /// </summary>
        [Reactive]
        public ObservableCollection<ItemEntryViewModel> GameModes { get; set; }
        
        /// <summary>
        /// A <see cref="ObservableCollection{T}" /> that contains all the selectable age rating filters.
        /// </summary>
        [Reactive]
        public ObservableCollection<ItemEntryViewModel> AgeRatings { get; set; }

        /// <summary>
        /// A <see cref="ObservableCollection{T}" /> that contains all the selectable status filters.
        /// </summary>
        [Reactive]
        public ObservableCollection<ItemEntryViewModel> Statuses { get; set; }

        /// <summary>
        /// Command that is invoked each time the page is first navigated to. When called, the command will
        /// propagate the request and call the <see cref="LoadGameFiltersAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadGameFiltersCommand { get; }
        
        /// <summary>
        /// Command that is invoked each time the user taps the "Platform" group header on the view. When called,
        /// the command switch the <see cref="IsPlatformGroupExpanded"/> state.
        /// </summary>
        public ReactiveCommand<bool, Unit> PlatformGroupTappedCommand { get; }
        
        /// <summary>
        /// Command that is invoked each time the user taps the "Genre" group header on the view. When called,
        /// the command switch the <see cref="IsGenreGroupExpanded"/> state.
        /// </summary>
        public ReactiveCommand<bool, Unit> GenreGroupTappedCommand { get; }
        
        /// <summary>
        /// Command that is invoked each time the user taps the "Game mode" group header on the view. When called,
        /// the command switch the <see cref="IsGameModeGroupExpanded"/> state.
        /// </summary>
        public ReactiveCommand<bool, Unit> GameModeGroupTappedCommand { get; }
        
        /// <summary>
        /// Command that is invoked each time the user taps the "Age rating" group header on the view. When called,
        /// the command switch the <see cref="IsAgeRatingGroupExpanded"/> state.
        /// </summary>
        public ReactiveCommand<bool, Unit> AgeRatingGroupTappedCommand { get; }

        /// <summary>
        /// Command that is invoked each time the user taps the "Age rating" group header on the view. When called,
        /// the command switch the <see cref="IsStatusGroupExpanded"/> state.
        /// </summary>
        public ReactiveCommand<bool, Unit> StatusGroupTappedCommand { get; }

        /// <summary>
        /// Command that is invoked any of the items within the "Platforms" group is tapped. When tapped, it will
        /// store the state of the current checked status in the <see cref="ItemEntryViewModel"/> and update the
        /// view with the new information.
        /// </summary>
        public ReactiveCommand<ItemEntryViewModel, Unit> PlatformTappedCommand { get; }
        
        /// <summary>
        /// Command that is invoked any of the items within the "Genre" group is tapped. When tapped, it will
        /// store the state of the current checked status in the <see cref="ItemEntryViewModel"/> and update the
        /// view with the new information.
        /// </summary>
        public ReactiveCommand<ItemEntryViewModel, Unit> GenreTappedCommand { get; }
        
        /// <summary>
        /// Command that is invoked any of the items within the "Game mode" group is tapped. When tapped, it will
        /// store the state of the current checked status in the <see cref="ItemEntryViewModel"/> and update the
        /// view with the new information.
        /// </summary>
        public ReactiveCommand<ItemEntryViewModel, Unit> GameModeTappedCommand { get; }
        
        /// <summary>
        /// Command that is invoked any of the items within the "Age rating" group is tapped. When tapped, it will
        /// store the state of the current checked status in the <see cref="ItemEntryViewModel"/> and update the
        /// view with the new information.
        /// </summary>
        public ReactiveCommand<ItemEntryViewModel, Unit> AgeRatingTappedCommand { get; }
        
        /// <summary>
        /// Command that is invoked any of the items within the "Status" group is tapped. When tapped, it will
        /// store the state of the current checked status in the <see cref="ItemEntryViewModel"/> and update the
        /// view with the new information.
        /// </summary>
        public ReactiveCommand<ItemEntryViewModel, Unit> StatusTappedCommand { get; }
        
        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
        
        /// <summary>
        /// Overriden method that is automatically invoked when the page is navigated to. Its purpose is to retrieve
        /// values from the <see cref="NavigationParameters" /> before invoking the <see cref="LoadGameFiltersCommand" />
        /// command to load and display the game filter information on the view.
        /// </summary>
        /// <param name="parameters">The <see cref="NavigationParameters" />, which contains information for display purposes.</param>
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.New)
            {
                // Load the game filters immediately on navigation.
                LoadGameFiltersCommand.Execute().Subscribe();
            }
        }
        
         /// <summary>
        /// Private method that is invoked by the <see cref="LoadGameFiltersCommand" /> when activated by the associated
        /// view. This method will attempt to retrieve the game filter from the url provided by the
        /// <see cref="NavigationParameters" /> and populate all of the information within this view model with data
        /// from the returned <see cref="GameUserEntryFilters" />. If any errors occur during the API requests, the exceptions
        /// are caught and the errors the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task" /> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoadGameFiltersAsync()
        {
            // Remove any previous error messages before loading.
            IsError = false;
            HasLoaded = false;
 
            // Retrieve the game and set some game information on the view.
            var gameUserEntryFilters = await _restService.GetAsync<GameUserEntryFilters>("games/entries/filters");
            
            // Convert the platform filters into a selectable list.
            Platforms = new ObservableCollection<ItemEntryViewModel>(gameUserEntryFilters.Platforms.Select(p => new ItemEntryViewModel
            {
                Id = p.Id,
                Name = p.Name
            }).ToList());
            
            // Convert the genre filters into a selectable list.
            Genres = new ObservableCollection<ItemEntryViewModel>(gameUserEntryFilters.Genres.Select(p => new ItemEntryViewModel
            {
                Id = p.Id,
                Name = p.Name
            }).ToList());
            
            // Convert the game mode filters into a selectable list.
            GameModes = new ObservableCollection<ItemEntryViewModel>(gameUserEntryFilters.GameModes.Select(p => new ItemEntryViewModel
            {
                Id = (int)p,
                Name = p.GetAttributeValue<DescriptionAttribute, string>(s => s.Description)
            }).ToList());
            
            // Convert the age rating filters into a selectable list.
            AgeRatings = new ObservableCollection<ItemEntryViewModel>(gameUserEntryFilters.AgeRatings.Select(p => new ItemEntryViewModel
            {
                Id = (int)p,
                Name = p.GetAttributeValue<DescriptionAttribute, string>(s => s.Description)
            }).ToList());

            // Convert the age rating filters into a selectable list.
            Statuses = new ObservableCollection<ItemEntryViewModel>(gameUserEntryFilters.Statuses.Select(p => new ItemEntryViewModel
            {
                Id = (int)p,
                Name = p.GetAttributeValue<DescriptionAttribute, string>(s => s.Description)
            }).ToList());

            HasLoaded = true;
        }
         
         /// <summary>
         /// Private method that is invoked by the <see cref="ApplyCommand"/>. When invoked, it will navigate the user
         /// to a game user list page filtered by the parameters specified within this view model.
         /// </summary>
         /// <returns></returns>
         private async Task ApplyAsync()
         {
             var platformIds = Platforms.Where(p => p.IsSelected)
                 .Select(p => p.Id)
                 .ToList();
             
             var genreIds = Genres.Where(g => g.IsSelected)
                 .Select(g => g.Id)
                 .ToList();

             var gameModes = GameModes.Where(g => g.IsSelected)
                 .Select(g => ((GameMode)g.Id).GetAttributeValue<EnumMemberAttribute, string>(e => e.Value))
                 .ToList();

             var ageRatings = AgeRatings.Where(a => a.IsSelected)
                 .Select(a => ((AgeRating) a.Id).GetAttributeValue<EnumMemberAttribute, string>(e => e.Value))
                 .ToList();
             
             var statuses = Statuses.Where(s => s.IsSelected)
                 .Select(s => ((GameUserEntryStatus) s.Id).GetAttributeValue<EnumMemberAttribute, string>(e => e.Value))
                 .ToList();
             
             var url = "games/entries/search" + 
                       "?platform-ids" + (platformIds.Count > 0 ? $"={string.Join(",", platformIds)}" : string.Empty) +
                       "&genre-ids" + (genreIds.Count > 0 ? $"={string.Join(",", genreIds)}" : string.Empty) +
                       "&game-modes" + (gameModes.Count > 0 ? $"={string.Join(",", gameModes)}" : string.Empty) +
                       "&age-ratings" + (ageRatings.Count > 0 ? $"={string.Join(",", ageRatings)}" : string.Empty) +
                       "&statuses" + (statuses.Count > 0 ? $"={string.Join(",", statuses)}" : string.Empty);
             
             var result = await NavigationService.NavigateAsync("GameUserEntryListPage", new NavigationParameters
             {
                 { "base-url", url }
             });
         }
    }
}