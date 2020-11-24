using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Microsoft.AppCenter.Crashes;
using Prism.Commands;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Common;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using ZXing;

namespace SparkyStudios.TrakLibrary.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameBarcodeScannerViewModel"/> is a view model that is associated with the game barcode scanner view.
    /// Its responsibility is to retrieve information from a scanned barcode and query the game library for a game that
    /// matches the scanned information. 
    /// </summary>
    public class GameBarcodeScannerViewModel : NonReactiveViewModel
    {
        private readonly IFormsDevice _formsDevice;
        private readonly IRestService _restService;
        private readonly IUserDialogs _userDialogs;
        
        private bool _isAnalyzing;
        
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService" /> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService" /> instance to inject.</param>
        /// <param name="userDialogs">The <see cref="IUserDialogs" /> instance to inject.</param>
        /// <param name="formsDevice">The <see cref="IFormsDevice" /> instance to inject.</param>
        public GameBarcodeScannerViewModel(INavigationService navigationService,
            IRestService restService, IUserDialogs userDialogs, IFormsDevice formsDevice) : base(navigationService)
        {
            _restService = restService;
            _userDialogs = userDialogs;
            _formsDevice = formsDevice;
        }
        
        /// <summary>
        /// A <see cref="bool"/> that controls whether the scanner is currently processing the information from a scanned
        /// barcode.
        /// </summary>
        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set => SetProperty(ref _isAnalyzing, value);
        }
        
        /// <summary>
        /// The <see cref="Result" /> which contains the scanned barcode information. It is set whenever a barcode is scanned
        /// by the scanner and is set before the <see cref="ScanCommand" /> is invoked.
        /// </summary>
        public Result Result { get; set; }
        
        /// <summary>
        /// A <see cref="ICommand" /> that is invoked by the view when a game barcode has been scanned. When called,
        /// the command will propagate the request and call the <see cref="ScanAsync" /> method. The <see cref="ICommand" />
        /// will only be invoked when the <see cref="IsBusy" /> boolean is set to false.
        /// </summary>
        public ICommand ScanCommand => new DelegateCommand(async () => await ScanAsync(), () => IsAnalyzing);

        /// <summary>
        /// Invoked when the view first navigates to the view associated with this view model. It will reset the
        /// <see cref="IsBusy"/> and <see cref="IsAnalyzing"/> boolean values to false and true respectively.
        /// </summary>
        /// <param name="parameters">The <see cref="INavigationParameters"/> sent when navigated to.</param>
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            IsBusy = false;
            IsAnalyzing = true;
        }

        /// <summary>
        /// Private method that is invoked when the <see cref="ScanCommand" /> is invoked by the view. Its' purpose is to
        /// retrieve the scanned barcode information and attempt to find a product linked to the barcode. If the barcode is
        /// mapped to a game within the game library, the user is re-directed to that page, where they can then add it to
        /// their own collection.
        ///
        /// If an expected error is thrown, such as an <see cref="ApiException" /> with the status code of a 404, this means
        /// the barcode has not been found and the user can then request for the game to be added to the library via the
        /// game request page. If any other errors of status codes are thrown, an error will be displayed to the user with
        /// a prompt to try again.
        /// </summary>
        /// <returns>A <see cref="Task" /> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ScanAsync()
        {
            IsAnalyzing = false;
            IsBusy = true;

            try
            {
                await AttemptBarcodeScanAsync();
            }
            catch (ApiException e)
            {
                HandleApiException(e);
                IsBusy = false;
            }
            catch (Exception e)
            {
                HandleException(e);
                IsBusy = false;
            }
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="ScanAsync" /> method. This method will retrieve the users
        /// authentication token and attempt to find a game within the game library that contains the scanned barcode.
        /// if no game is found, the exception is propagated up the stack.
        ///
        /// If the barcode matches to an existing game within the library, the user is re-directed to the game page
        /// where they can add the game to their collection or view similar products.
        /// </summary>
        /// <returns>A <see cref="Task" /> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task AttemptBarcodeScanAsync()
        {
            // Check to see if the barcode scanned matches any stored in the server.
            // If it doesn't, it'll throw an exception.
            var gameBarcode =
                await _restService.GetAsync<GameBarcode>(
                    $"games/barcodes/{Result.Text}");

            // Set the needed parameters to correctly load the game page for the 
            // given barcode.
            var parameters = new NavigationParameters
            {
                {"game-url", gameBarcode.GetLink("gameDetails")}
            };

            _formsDevice.BeginInvokeOnMainThread(async () =>
                await NavigationService.NavigateAsync("GamePage", parameters));
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="ScanAsync" /> method when the
        /// <see cref="AttemptBarcodeScanAsync" /> throws a <see cref="ApiException" />. This method
        /// will check the status code of the thrown <see cref="ApiException" />, if the status code is a
        /// 404, then the user will be prompted to make a request for the game to be added to the game
        /// library and re-directed to the game request page.
        ///
        /// If the exception thrown doesn't have a status code of 404, we can assume a transient error occurred and the user is
        /// prompted to retry the request.
        /// </summary>
        /// <param name="e">The <see cref="ApiException" /> thrown by the <see cref="AttemptBarcodeScanAsync" /> method.</param>
        private void HandleApiException(ApiException e)
        {
            // This will be returned if the barcode is valid but not mapped to a game within the library.
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                // Prompt the user to either go back or navigate to the game request page.
                var promptConfig = new PromptConfig()
                    .SetTitle(Messages.TrakTitle)
                    .SetMessage(Messages.GameBarcodeScannerPageRequestGame)
                    .SetOkText(Messages.Yes)
                    .SetCancelText(Messages.No);

                _formsDevice.BeginInvokeOnMainThread(async () =>
                {
                    var result = await _userDialogs.PromptAsync(promptConfig);
                    if (result.Ok)
                    {
                        await NavigationService.NavigateAsync("GameRequestPage");
                    }
                    else
                    {
                        IsAnalyzing = true;
                    }
                });
            }
            else
            {
                var alertConfig = new AlertConfig()
                    .SetTitle(Messages.TrakTitle)
                    .SetMessage(Messages.GameBarcodeScannerPageApiError);

                _formsDevice.BeginInvokeOnMainThread(async () =>
                {
                    await _userDialogs.AlertAsync(alertConfig);
                    
                    IsAnalyzing = true;
                });
            }
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="ScanAsync" /> method when the <see cref="AttemptBarcodeScanAsync" />
        /// throws an <see cref="Exception" />. If an unknown <see cref="Exception" /> is thrown, it can be assumed that an
        /// implementation error has occurred and an error will be reported via App Center.
        ///
        /// The user will be presented with an error message telling them that an error has occurred, although there is no
        /// recommendation to retry.
        /// </summary>
        private void HandleException(Exception e)
        {
            var alertConfig = new AlertConfig()
                .SetTitle(Messages.TrakTitle)
                .SetMessage(Messages.GameBarcodeScannerPageGenericError);

            _formsDevice.BeginInvokeOnMainThread(async () =>
            {
                await _userDialogs.AlertAsync(alertConfig);
                IsAnalyzing = true;
            });
            
            Crashes.TrackError(e, new Dictionary<string, string>
            {
                {"Barcode", Result.Text}
            });
        }
    }
}