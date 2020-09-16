using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reactive.Concurrency;
using Acr.UserDialogs;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.IdentityModel.Tokens;
using Prism;
using Prism.Ioc;
using Prism.Plugin.Popups;
using ReactiveUI;
using SparkyStudios.TrakLibrary.Helpers;
using SparkyStudios.TrakLibrary.Impl;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.ViewModel;
using SparkyStudios.TrakLibrary.ViewModel.Games;
using SparkyStudios.TrakLibrary.ViewModel.Login;
using SparkyStudios.TrakLibrary.ViewModel.Settings;
using SparkyStudios.TrakLibrary.Views;
using SparkyStudios.TrakLibrary.Views.Games;
using SparkyStudios.TrakLibrary.Views.Login;
using SparkyStudios.TrakLibrary.Views.Settings;

namespace SparkyStudios.TrakLibrary
{
    public partial class App
    {
        public App() : this(null)
        {
        }

        public App(IPlatformInitializer initializer) : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            AppCenter.Start($"android={Secrets.AppCenterAndroidSecret};ios={Secrets.AppCenterIOSSecret};",
                typeof(Analytics), typeof(Crashes));

            // Get the storage service.
            var storageService = Container.Resolve<IStorageService>();

            // We need to check if a unique device ID has been generated, if not generate one and persist it.
            // It'll be used for push notification purposes.
            var deviceId = await storageService.GetDeviceIdAsync();
            if (deviceId.Equals(Guid.Empty))
            {
                await storageService.SetDeviceIdAsync(Guid.NewGuid());
            }
            
            await NavigationService.NavigateAsync("LoadingPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterPopupNavigationService<TransitionPopupPageNavigationService>();

            // Services
            containerRegistry.Register<IFormsDevice, XamarinFormsDevice>();
            containerRegistry.Register<IStorageService, SecureStorageService>();
            containerRegistry.RegisterInstance(UserDialogs.Instance);
            containerRegistry.RegisterInstance(typeof(IScheduler), RxApp.MainThreadScheduler);
            containerRegistry.RegisterInstance(typeof(SecurityTokenHandler), new JwtSecurityTokenHandler());
            ServiceRegistry.RegisterTypes(containerRegistry, Container.Resolve<IStorageService>(), Secrets.EnvironmentUrl);

            // Xamarin pages.
            containerRegistry.RegisterForNavigation<BaseMasterDetailPage, BaseMasterDetailViewModel>();
            containerRegistry.RegisterForNavigation<BaseNavigationPage>();

            // Login pages.
            containerRegistry.RegisterForNavigation<LoadingPage, LoadingViewModel>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginViewModel>();
            containerRegistry.RegisterForNavigation<RegisterPage, RegisterViewModel>();
            containerRegistry.RegisterForNavigation<ForgottenPasswordPage, ForgottenPasswordViewModel>();
            containerRegistry.RegisterForNavigation<RecoveryPage, RecoveryViewModel>();
            containerRegistry.RegisterForNavigation<VerificationPage, VerificationViewModel>();

            // Home pages
            containerRegistry.RegisterForNavigation<HomePage, HomeViewModel>();
            
            // Settings pages
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>();
            containerRegistry.RegisterForNavigation<ChangeEmailAddressPage, ChangeEmailAddressViewModel>();
            containerRegistry.RegisterForNavigation<ChangePasswordPage, ChangePasswordViewModel>();
            containerRegistry.RegisterForNavigation<RequestChangePasswordPage, RequestChangePasswordViewModel>();
            
            // Game pages
            containerRegistry.Register<GameUserEntryBacklogListViewModel>();
            containerRegistry.Register<GameUserEntryInProgressListViewModel>();
            containerRegistry.Register<GameUserEntryCompletedListViewModel>();
            containerRegistry.Register<GameUserEntryDroppedListViewModel>();
            containerRegistry.RegisterForNavigation<GameUserEntriesTabbedPage, GameUserEntriesTabbedViewModel>();
            containerRegistry.RegisterForNavigation<GameLibraryTabbedPage>();
            containerRegistry.RegisterForNavigation<GameLibraryListPage, GameLibraryListViewModel>();
            containerRegistry.RegisterForNavigation<GameRequestPage, GameRequestViewModel>();
            containerRegistry.RegisterForNavigation<GameBarcodeScannerPage, GameBarcodeScannerViewModel>();
            containerRegistry.RegisterForNavigation<GamePage, GameViewModel>();
            containerRegistry.RegisterForNavigation<AddGamePage, AddGameViewModel>();
            containerRegistry.RegisterForNavigation<GameOptionsPage, GameOptionsViewModel>();
        }
    }
}