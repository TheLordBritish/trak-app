﻿using System;
using System.Reactive.Concurrency;
using Acr.UserDialogs;
using Prism;
using Prism.Ioc;
using Prism.Plugin.Popups;
using ReactiveUI;
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
            ServiceRegistry.RegisterTypes(containerRegistry);
            containerRegistry.Register<IFormsDevice, XamarinFormsDevice>();
            containerRegistry.Register<IStorageService, SecureStorageService>();
            containerRegistry.RegisterInstance(UserDialogs.Instance);
            containerRegistry.RegisterInstance(typeof(IScheduler), RxApp.MainThreadScheduler);
            
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
