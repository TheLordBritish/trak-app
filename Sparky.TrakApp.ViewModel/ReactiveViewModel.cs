using Prism.Navigation;
using ReactiveUI;

namespace Sparky.TrakApp.ViewModel
{
    public abstract class ReactiveViewModel : ReactiveObject, IInitialize, INavigationAware, IDestructible
    {
        private bool _isError;
        private string _errorMessage;
        
        protected ReactiveViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }
        
        protected INavigationService NavigationService { get; }

        public bool IsError
        {
            get => _isError;
            set => this.RaiseAndSetIfChanged(ref _isError, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public virtual void Initialize(INavigationParameters parameters)
        {
            
        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
            
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
            
        }

        public virtual void Destroy()
        {
            
        }
    }
}