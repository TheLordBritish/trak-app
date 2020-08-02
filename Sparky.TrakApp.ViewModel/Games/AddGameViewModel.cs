using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentValidation;
using Microsoft.AppCenter.Crashes;
using Plugin.FluentValidationRules;
using Prism.Commands;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Games.Validation;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Common;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Games
{
    public class AddGameViewModel : ReactiveViewModel, IValidate<AddGameDetails>
    {
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;
        
        private bool _inLibrary;

        private IValidator _validator;
        private Validatables _validatables;
        
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        public AddGameViewModel(IScheduler scheduler, INavigationService navigationService, IStorageService storageService,
            IRestService restService) : base(scheduler, navigationService)
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
            
            SetupForValidation();
            
            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation);
            
            AddGameCommand = ReactiveCommand.CreateFromTask(AddGameAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            AddGameCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (ex is ApiException)
                {
                    ErrorMessage = Messages.ErrorMessageApiError;
                }
                else
                {
                    ErrorMessage = Messages.ErrorMessageGeneric;
                    Crashes.TrackError(ex, new Dictionary<string, string>
                    {
                        {"Game ID", GameId.ToString()},
                        {"Platform ID", SelectedPlatform.Value.Id.ToString()}
                    });
                }
            });

            this.WhenAnyObservable(x => x.AddGameCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);
        }
        
        /// <summary>
        /// A <see cref="long"/> that represents the ID of the currently selected <see cref="Game"/> to add.
        /// </summary>
        public long GameId { get; private set; }
        
        /// <summary>
        /// The <see cref="Uri"/> that is used to retrieve additional information about the <see cref="Game"/>
        /// that is to be added to the users collection.
        /// </summary>
        public Uri GameUrl { get; private set; }
        
        /// <summary>
        /// An <see cref="ObservableCollection{T}"/> that contains all of the <see cref="Platform"/>'s
        /// that the current <see cref="Game"/> is available on.
        /// </summary>
        [Reactive]
        public ObservableCollection<Platform> Platforms { get; private set; }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently selected platform with additional
        /// validation information.
        /// </summary>
        [Reactive]
        public Validatable<Platform> SelectedPlatform { get; set; }

        /// <summary>
        /// An <see cref="ObservableCollection{T}"/> that contains a list of all valid and selectable
        /// <see cref="GameUserEntryStatus"/> enumerations.
        /// </summary>
        [Reactive]
        public ObservableCollection<GameUserEntryStatus> Statuses { get; private set; }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently selected status with additional
        /// validation information.
        /// </summary>
        [Reactive]
        public Validatable<GameUserEntryStatus> SelectedStatus { get; set; }
        
        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="AddGameViewModel"/> is the platform and status. When the view is changed,
        /// the name is passed through and the request propagated to the <see cref="ClearValidation"/>
        /// method.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }

        /// <summary>
        /// Command that is invoked each the time the Add button is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="AddGameAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddGameCommand { get; }

        /// <summary>
        /// Invoked when the view associated with this view model is being removed from the view. It will add
        /// a number of parameters to the <see cref="INavigationParameters"/> instance provided referencing the
        /// game selected, as this information is needed when navigating back to the game view.
        /// </summary>
        /// <param name="parameters">The <see cref="INavigationParameters"/> sent when navigating away.</param>
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            parameters.Add("game-url", GameUrl);
            parameters.Add("platform-id", _inLibrary ? SelectedPlatform.Value.Id : 0L);
            parameters.Add("in-library", _inLibrary);
            parameters.Add("status", _inLibrary ? SelectedStatus.Value : GameUserEntryStatus.None);
        }

        /// <summary>
        /// Invoked when the view first navigates to the view associated with this view model. It will retrieve
        /// the game information provided by the <see cref="INavigationParameters"/> and the associated platforms
        /// of the <see cref="Game"/>.
        /// </summary>
        /// <param name="parameters">The <see cref="INavigationParameters"/> sent when navigated to.</param>
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            GameUrl = parameters.GetValue<Uri>("game-url");
            GameId = parameters.GetValue<long>("game-id");

            Platforms = new ObservableCollection<Platform>(parameters.GetValue<IEnumerable<Platform>>("platforms"));
        }

        /// <summary>
        /// Invoked within the constructor of the <see cref="AddGameViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="AddGameDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            SelectedPlatform = new Validatable<Platform>(nameof(AddGameDetails.Platform));
            SelectedStatus = new Validatable<GameUserEntryStatus>(nameof(AddGameDetails.Status))
            {
                Value = GameUserEntryStatus.Backlog
            };
            
            _validator = new AddGameDetailsValidator();
            _validatables = new Validatables(SelectedPlatform, SelectedStatus);
        }

        /// <summary>
        /// Validates the specified <see cref="AddGameDetails"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="AddGameDetailsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="AddGameDetails"/> instance to validate against the <see cref="AddGameDetailsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(AddGameDetails model)
        {
            return _validator.Validate(model)
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="AddGameViewModel"/>.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="AddGameCommand"/> when activated by the associated
        /// view. This method will validate the selected platform and status on the view, and if valid attempt to add
        /// the specified game to the users collection.
        ///
        /// If any errors occur during validation or authentication, the exceptions are caught and the errors are
        /// displayed to the user through the ErrorMessage parameter and setting the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AddGameAsync()
        {
            IsError = false;

            var details = _validatables.Populate<AddGameDetails>();
            var validationResult = Validate(details);
            
            if (validationResult.IsValidOverall)
            {
                await AttemptAddGameAsync();
            }
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="AddGameAsync"/> method. Its purpose is to call off to the
        /// Trak API and either add the game to the users collection, or update it if the information provided already matches
        /// and existing entry.
        ///
        /// Once the game has been added or updated, the user will be navigated back to the previous page, closing the dialog
        /// and reloading the game view. Once a game has been added, it cannot be added again, instead the user will be presented
        /// with options to edit the existing entry.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AttemptAddGameAsync()
        {
            // Get the needed values to make the call from the storage service.
            var userId = await _storageService.GetUserIdAsync();
            var token = await _storageService.GetAuthTokenAsync();

            // Make the request to see if the game they're adding is already in their library.
            var existingEntry = await _restService.GetAsync<HateoasPage<GameUserEntry>>(
                $"api/game-management/v1/game-user-entries?user-id={userId}&platform-id={SelectedPlatform.Value.Id}&game-id={GameId}",
                token);

            if (existingEntry.Embedded != null)
            {
                // If the entry is already in the users library, just update it with the selected status they provided.
                var entry = existingEntry.Embedded.Data.First();
                entry.Status = SelectedStatus.Value;

                // Make a request to update the game to their collection.
                await _restService.PutAsync("api/game-management/v1/game-user-entries", entry, token);
            }
            else
            {
                var entry = new GameUserEntry
                {
                    UserId = userId,
                    GameId = GameId,
                    PlatformId = SelectedPlatform.Value.Id,
                    Status = SelectedStatus.Value
                };

                // Make a request to add the game to their collection.
                await _restService.PostAsync("api/game-management/v1/game-user-entries", entry, token);
            }

            _inLibrary = true;
            await NavigationService.GoBackAsync();
        }
    }
}