using Acr.UserDialogs;
using Prism;
using Prism.Ioc;
using Sparky.TrakApp.Impl;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.ViewModel;
using Sparky.TrakApp.ViewModel.Games;
using Sparky.TrakApp.ViewModel.Login;
using Sparky.TrakApp.Views;
using Sparky.TrakApp.Views.Games;
using Sparky.TrakApp.Views.Login;

namespace Sparky.TrakApp
{
    public partial class App
    {
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();
            await NavigationService.NavigateAsync(nameof(LoginPage));
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Services
            ServiceRegistry.RegisterTypes(containerRegistry);
            containerRegistry.Register<IFormsDevice, XamarinFormsDevice>();
            containerRegistry.Register<IStorageService, SecureStorageService>();
            containerRegistry.RegisterInstance(UserDialogs.Instance);
            
            // Xamarin pages.
            containerRegistry.RegisterForNavigation<BaseMasterDetailPage, BaseMasterDetailViewModel>();
            containerRegistry.RegisterForNavigation<BaseNavigationPage>();
            
            // Login pages.
            containerRegistry.RegisterForNavigation<LoginPage, LoginViewModel>();
            containerRegistry.RegisterForNavigation<RegisterPage, RegisterViewModel>();
            containerRegistry.RegisterForNavigation<ForgottenPasswordPage, ForgottenPasswordViewModel>();
            containerRegistry.RegisterForNavigation<RecoveryPage, RecoveryViewModel>();
            containerRegistry.RegisterForNavigation<VerificationPage, VerificationViewModel>();
            
            // Home pages
            containerRegistry.RegisterForNavigation<HomePage, HomeViewModel>();
            
            // Game pages
            containerRegistry.Register<GameUserEntryBacklogListViewModel>();
            containerRegistry.Register<GameUserEntryInProgressListViewModel>();
            containerRegistry.Register<GameUserEntryCompletedListViewModel>();
            containerRegistry.Register<GameUserEntryDroppedListViewModel>();
            containerRegistry.RegisterForNavigation<GameUserEntriesTabbedPage, GameUserEntriesTabbedViewModel>();
            containerRegistry.RegisterForNavigation<GameLibraryTabbedPage>();
            containerRegistry.RegisterForNavigation<GameBarcodeScannerPage, GameBarcodeScannerViewModel>();
            containerRegistry.RegisterForNavigation<GamePage, GameViewModel>();
        }
    }
}
