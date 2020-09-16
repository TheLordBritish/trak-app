using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reactive.Concurrency;
using Microsoft.IdentityModel.Tokens;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.ViewModel.Common;

namespace SparkyStudios.TrakLibrary.ViewModel.Login
{
    /// <summary>
    /// The <see cref="LoadingViewModel"/> is a simple view model that is associated with the loading page view.
    /// Its responsibility is to load any existing credentials when the app first loads and either direct the user
    /// to the home page when the app contains valid credentials, or go to the login page if any problems occured.
    /// </summary>
    public class LoadingViewModel : ReactiveViewModel
    {
        private readonly IStorageService _storageService;
        private readonly IRestService _restService;
        private readonly SecurityTokenHandler _securityTokenHandler;

        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        /// <param name="securityTokenHandler">The <see cref="SecurityTokenHandler"/> instance to inject.</param>
        public LoadingViewModel(IScheduler scheduler, INavigationService navigationService,
            IStorageService storageService, IRestService restService, SecurityTokenHandler securityTokenHandler) : base(scheduler, navigationService)
        {
            _storageService = storageService;
            _restService = restService;
            _securityTokenHandler = securityTokenHandler;
        }

        /// <summary>
        /// Method that is invoked when the page is first loaded. It's purpose is to search the store for
        /// existing credentials and try and retrieve a valid authentication token to automatically log the
        /// user in. If the user retrieved is either not verified or the credentials within the store are not
        /// valid, then the user will be navigated to the login page, otherwise they'll go straight to the
        /// home page.
        /// </summary>
        /// <param name="parameters"></param>
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                // Retrieve existing token.
                var token = await _storageService.GetAuthTokenAsync();

                // Only process if the user can log in if they actually have a token stored.
                if (!string.IsNullOrEmpty(token))
                {
                    // decode the jwt.
                    var jwt = _securityTokenHandler.ReadToken(token) as JwtSecurityToken;

                    // Get the needed information from the JWT.
                    var username = jwt.Subject;
                    var userId = int.Parse(jwt.Claims.First(c => c.Type == "userId").Value);
                    var verified = bool.Parse(jwt.Claims.First(c => c.Type == "verified").Value);
                    
                    // We'll need to update the information within the store just in case its changed since last the user 
                    // logged in.
                    await _storageService.SetUsernameAsync(username);
                    await _storageService.SetUserIdAsync(userId);
                    
                    // Need to ensure the correct details are registered for push notifications.
                    await _restService.PostAsync("notifications/register",
                        new NotificationRegistrationRequest
                        {
                            UserId = await _storageService.GetUserIdAsync(),
                            DeviceGuid = (await _storageService.GetDeviceIdAsync()).ToString(),
                            Token = await _storageService.GetNotificationTokenAsync()
                        });
                    
                    if (!verified)
                    {
                        await NavigationService.NavigateAsync("/LoginPage");
                    }
                    else
                    {
                        await NavigationService.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage");
                    }
                }
                else
                {
                    await NavigationService.NavigateAsync("/LoginPage");
                }
            }
            catch (Exception)
            {
                // Failed to get a token, just go back to the login page and make them login again.
                await NavigationService.NavigateAsync("/LoginPage");
            }
        }
    }
}