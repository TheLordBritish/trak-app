using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Sparky.TrakApp.ViewModel
{
    public abstract class ReactiveViewModel : ReactiveObject, IInitialize, INavigationAware, IDestructible
    {
        protected ReactiveViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }
        
        protected INavigationService NavigationService { get; }

        [Reactive]
        public bool IsError { get; protected set; }

        [Reactive]
        public string ErrorMessage { get; protected set; }

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