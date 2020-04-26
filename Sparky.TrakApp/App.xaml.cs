using Prism;
using Prism.Ioc;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.ViewModel.Login;
using Sparky.TrakApp.Views;
using Sparky.TrakApp.Views.Login;
using Xamarin.Forms;

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

            // Xamarin pages.
            containerRegistry.RegisterForNavigation<NavigationPage>();
            
            // Login pages.
            containerRegistry.RegisterForNavigation<LoginPage, LoginViewModel>();
            containerRegistry.RegisterForNavigation<RegisterPage, RegisterViewModel>();
            containerRegistry.RegisterForNavigation<VerificationPage, VerificationViewModel>();
            
            // Home pages
            containerRegistry.RegisterForNavigation<BaseMasterDetailPage>();
            containerRegistry.RegisterForNavigation<HomePage>();
        }
    }
}
