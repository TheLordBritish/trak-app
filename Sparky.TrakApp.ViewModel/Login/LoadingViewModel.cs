using System;
using Prism.Navigation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Service;

namespace Sparky.TrakApp.ViewModel.Login
{
    /// <summary>
    /// The <see cref="LoadingViewModel"/> is a simple view model that is associated with the loading page view.
    /// Its responsibility is to load any existing credentials when the app first loads and either direct the user
    /// to the home page when the app contains valid credentials, or go to the login page if any problems occured.
    /// </summary>
    public class LoadingViewModel : BaseViewModel
    {
        private readonly IStorageService _storageService;
        private readonly IAuthService _authService;
        
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="authService">The <see cref="IAuthService"/> instance to inject.</param>
        public LoadingViewModel(INavigationService navigationService, IStorageService storageService, IAuthService authService) : base(navigationService)
        {
            _storageService = storageService;
            _authService = authService;
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
                // Retrieve any existing credentials.
                var username = await _storageService.GetUsernameAsync();
                var password = await _storageService.GetPasswordAsync();
                
                // Try to retrieve a token from the credentials in the store.
                var token = await _authService.GetTokenAsync(new UserCredentials
                {
                    Username = username,
                    Password = password
                });
                
                // We got a valid token, so store it.
                await _storageService.SetAuthTokenAsync(token);
                
                // Need to get details to check if they're verified, if they're not they can go back
                // to the login page.
                var userResponse = await _authService.GetFromUsernameAsync(username, token);
                
                if (!userResponse.Verified)
                {
                    await NavigationService.NavigateAsync("/LoginPage");
                }
                else
                {
                    await NavigationService.NavigateAsync("/BaseMasterDetailPage/BaseNavigationPage/HomePage");
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