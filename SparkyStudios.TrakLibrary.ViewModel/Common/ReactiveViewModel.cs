using System.Reactive.Concurrency;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace SparkyStudios.TrakLibrary.ViewModel.Common
{
    public abstract class ReactiveViewModel : ReactiveObject, IInitialize, INavigationAware, IDestructible
    {
        protected ReactiveViewModel(IScheduler scheduler, INavigationService navigationService)
        {
            Scheduler = scheduler ?? RxApp.MainThreadScheduler;
            NavigationService = navigationService;
        }
        
        protected IScheduler Scheduler { get; }
        
        protected INavigationService NavigationService { get; }

        [Reactive]
        public bool IsError { get; protected set; }

        [Reactive]
        public string ErrorMessage { get; protected set; }

        public bool IsLoading { [ObservableAsProperty] get; }
        
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