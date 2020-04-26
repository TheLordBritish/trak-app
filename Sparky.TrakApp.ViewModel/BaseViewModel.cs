using Prism.Mvvm;
using Prism.Navigation;

namespace Sparky.TrakApp.ViewModel
{
    public class BaseViewModel : BindableBase, IInitialize, INavigationAware, IDestructible
    {
        private bool _isBusy;
        private bool _isError;
        
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

        public BaseViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
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
