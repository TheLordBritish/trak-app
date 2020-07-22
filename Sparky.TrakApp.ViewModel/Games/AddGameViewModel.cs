using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentValidation;
using Plugin.FluentValidationRules;
using Prism.Commands;
using Prism.Navigation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Games.Validation;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Games
{
    public class AddGameViewModel : BaseViewModel, IValidate<AddGameDetails>
    {
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;

        private Uri _gameUrl;
        private long _gameId;
        private bool _inLibrary;

        private IValidator _validator;
        private Validatables _validatables;

        private ObservableCollection<Platform> _platforms;
        private Validatable<Platform> _selectedPlatform;

        private ObservableCollection<GameUserEntryStatus> _statuses;
        private Validatable<GameUserEntryStatus> _selectedStatus;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        public AddGameViewModel(INavigationService navigationService, IStorageService storageService,
            IRestService restService) : base(navigationService)
        {
            _storageService = storageService;
            _restService = restService;

            Platforms = new ObservableCollection<Platform>();
            Statuses = new ObservableCollection<GameUserEntryStatus>
            {
                GameUserEntryStatus.Backlog,
                GameUserEntryStatus.InProgress,
                GameUserEntryStatus.Completed,
                GameUserEntryStatus.Dropped
            };

            SelectedPlatform = new Validatable<Platform>(nameof(AddGameDetails.Platform));
            SelectedStatus = new Validatable<GameUserEntryStatus>(nameof(AddGameDetails.Status))
            {
                Value = GameUserEntryStatus.Backlog
            };

            SetupForValidation();
        }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="AddGameViewModel"/> is the platform and status. When the view is changed,
        /// the name is passed through and the request propagated to the <see cref="ClearValidation"/>
        /// method.
        /// </summary>
        public ICommand ClearValidationCommand => new DelegateCommand<string>(ClearValidation);

        /// <summary>
        /// Command that is invoked each the time the Add button is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="AddGameAsync"/> method.
        /// </summary>
        public ICommand AddGameCommand => new DelegateCommand(async () => await AddGameAsync());
        
        public ObservableCollection<Platform> Platforms
        {
            get => _platforms;
            set => SetProperty(ref _platforms, value);
        }

        public Validatable<Platform> SelectedPlatform
        {
            get => _selectedPlatform;
            set => SetProperty(ref _selectedPlatform, value);
        }

        public ObservableCollection<GameUserEntryStatus> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        public Validatable<GameUserEntryStatus> SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            parameters.Add("game-url", _gameUrl);
            parameters.Add("platform-id", _inLibrary ? SelectedPlatform.Value.Id : 0L);
            parameters.Add("in-library", _inLibrary);
            parameters.Add("status", _inLibrary ? SelectedStatus.Value : GameUserEntryStatus.None);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _gameUrl = parameters.GetValue<Uri>("game-url");
            _gameId = parameters.GetValue<long>("game-id");

            Platforms = new ObservableCollection<Platform>(parameters.GetValue<IEnumerable<Platform>>("platforms"));
        }

        /// <summary>
        /// Invoked within the constructor of the <see cref="AddGameViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="AddGameDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
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
            return _validator.Validate(model).ApplyResultsTo(_validatables);
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

        private async Task AddGameAsync()
        {
            IsError = false;
            IsBusy = true;

            var details = _validatables.Populate<AddGameDetails>();
            var validationResult = Validate(details);

            try
            {
                if (validationResult.IsValidOverall)
                {
                    await AttemptAddGameAsync();
                }
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

        private async Task AttemptAddGameAsync()
        {
            // Get the needed values to make the call from the storage service.
            var userId = await _storageService.GetUserIdAsync();
            var token = await _storageService.GetAuthTokenAsync();

            // Make the request to see if the game they're adding is already in their library.
            var existingEntry = await _restService.GetAsync<HateoasPage<GameUserEntry>>(
                $"api/game-management/v1/game-user-entries?user-id={userId}&platform-id={SelectedPlatform.Value.Id}&game-id={_gameId}",
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
                    GameId = _gameId,
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