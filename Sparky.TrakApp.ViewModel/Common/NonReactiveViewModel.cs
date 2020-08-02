using Prism.Mvvm;
using Prism.Navigation;

namespace Sparky.TrakApp.ViewModel.Common
{
    public abstract class NonReactiveViewModel : BindableBase, IInitialize, INavigationAware, IDestructible
    {
        private bool _isBusy;
        private bool _isError;
        private string _errorMessage;

        protected NonReactiveViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }
        
        protected INavigationService NavigationService { get; }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
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
