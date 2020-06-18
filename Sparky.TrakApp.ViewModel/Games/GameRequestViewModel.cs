using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Acr.UserDialogs;
using FluentValidation;
using Plugin.FluentValidationRules;
using Prism.Navigation;
using ReactiveUI;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Resources;
using Sparky.TrakApp.ViewModel.Validation;

namespace Sparky.TrakApp.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameRequestViewModel"/> is a simple view model that is associated with the game request page view.
    /// Its responsibility is to respond to user-made game requests made with a title and any additional notes.
    ///
    /// The <see cref="GameRequestViewModel"/> also provides methods to validate fields on the game request page view.
    /// Any validation errors or generic errors are stored within the view model for use on the view.
    /// </summary>
    public class GameRequestViewModel : ReactiveViewModel, IValidate<GameRequestDetails>
    {
        private readonly IRestService _restService;
        private readonly IStorageService _storageService;
        private readonly IUserDialogs _userDialogs;

        private Validatable<string> _title;
        private Validatable<string> _notes;

        private IValidator _validator;
        private Validatables _validatables;

        private readonly ObservableAsPropertyHelper<bool> _isLoading;

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
        public GameRequestViewModel(IScheduler scheduler, INavigationService navigationService,
            IRestService restService,
            IStorageService storageService, IUserDialogs userDialogs) : base(navigationService)
        {
            scheduler = scheduler ?? RxApp.MainThreadScheduler;

            _restService = restService;
            _storageService = storageService;
            _userDialogs = userDialogs;

            SetupForValidation();

            ClearValidationCommand = ReactiveCommand.Create<string>(ClearValidation);

            RequestCommand = ReactiveCommand.CreateFromTask(ExecuteRequestAsync, outputScheduler: scheduler);

            // Report errors if an exception was thrown.
            RequestCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                ErrorMessage = ex is ApiException ? Messages.ErrorMessageApiError : Messages.ErrorMessageGeneric;
            });

            _isLoading = this.WhenAnyObservable(x => x.RequestCommand.IsExecuting)
                .ToProperty(this, x => x.IsLoading, scheduler: scheduler);
        }

        /// <summary>
        /// Command that is invoked each time that the validatable field on the view is changed, which
        /// for the <see cref="GameRequestViewModel"/> is the title and notes. When the view is changed,
        /// the name is passed through and the request propagated to the <see cref="ClearValidation"/>
        /// methods.
        /// </summary>
        public ReactiveCommand<string, Unit> ClearValidationCommand { get; }

        /// <summary>
        /// Command that is invoked by the view when the submit button is tapped. When called, the command
        /// will propagate the request and call the <see cref="ExecuteRequestAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RequestCommand { get; }

        /// <summary>
        /// A boolean represents whether the <see cref="RequestCommand"/> is currently executing the behaviour
        /// within the <see cref="ReactiveCommand{TParam,TResult}"/>.
        /// </summary>
        public bool IsLoading => _isLoading.Value;

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated title with
        /// additional validation information.
        /// </summary>
        public Validatable<string> Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        /// <summary>
        /// A <see cref="Validatable{T}"/> that contains the currently populated notes with
        /// additional validation information.
        /// </summary>
        public Validatable<string> Notes
        {
            get => _notes;
            set => this.RaiseAndSetIfChanged(ref _notes, value);
        }

        /// <summary>
        /// Invoked within the constructor of the <see cref="GameRequestViewModel"/>, its' responsibility is to
        /// instantiate the <see cref="AbstractValidator{T}"/> and the validatable values that will need to
        /// meet the specified criteria within the <see cref="GameRequestDetailsValidator"/> to pass validation.
        /// </summary>
        public void SetupForValidation()
        {
            _title = new Validatable<string>(nameof(GameRequestDetails.Title));
            _notes = new Validatable<string>(nameof(GameRequestDetails.Notes));

            _validator = new GameRequestDetailsValidator();
            _validatables = new Validatables(Title, Notes);
        }

        /// <summary>
        /// Validates the specified <see cref="GameRequestDetails"/> model with the validation rules specified within
        /// this class, which are contained within the <see cref="GameRequestDetailsValidator"/>. The results, regardless
        /// of whether they are true or false are applied to the validatable variable. 
        /// </summary>
        /// <param name="model">The <see cref="GameRequestDetails"/> instance to validate against the <see cref="GameRequestDetailsValidator"/>.</param>
        /// <returns>A <see cref="OverallValidationResult"/> which will contain a list of any errors.</returns>
        public OverallValidationResult Validate(GameRequestDetails model)
        {
            return _validator.Validate(model)
                .ApplyResultsTo(_validatables);
        }

        /// <summary>
        /// Clears the validation information for the specified variable within this <see cref="GameRequestViewModel"/>.
        /// If the clear options are sent through as an empty string, all validation information within this
        /// view model is cleared.
        /// </summary>
        /// <param name="clearOptions">Which validation information to clear from the context.</param>
        public void ClearValidation(string clearOptions = "")
        {
            _validatables.Clear(clearOptions);
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="RequestCommand"/> when activated by the associated
        /// view. This method will validate the title and notes on the view, and if valid attempt to create
        /// a new game request before displaying an alert to the user and navigating them back to the previous
        /// page.
        ///
        /// If any errors occur during validation or authentication, the exceptions are caught and the errors are
        /// displayed to the user through the ErrorMessage parameter and setting the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ExecuteRequestAsync()
        {
            IsError = false;

            var details = _validatables.Populate<GameRequestDetails>();
            var validationResult = Validate(details);

            // Only execute the request if the information provided is valid.
            if (validationResult.IsValidOverall)
            {
                // Get the current ID of the user, needed to register a request against their account.
                var userId = await _storageService.GetUserIdAsync();
                var authToken = await _storageService.GetAuthTokenAsync();

                // Make the request object to send.
                var gameRequest = new GameRequest
                {
                    Title = Title.Value,
                    Notes = Notes.Value,
                    UserId = userId
                };

                // Make the request to the server to create the new game request.
                await _restService.PostAsync("api/game-management/v1/game-requests", gameRequest, authToken);

                // On a successful request, show the user a confirmation dialog and navigate back to the previous page.
                var alertConfig = new AlertConfig()
                    .SetTitle(Messages.TrakTitle)
                    .SetMessage(Messages.GameRequestPageRequestComplete);

                await _userDialogs.AlertAsync(alertConfig);
                await NavigationService.GoBackAsync();
            }
        }
    }
}