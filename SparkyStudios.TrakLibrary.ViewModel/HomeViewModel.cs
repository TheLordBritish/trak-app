using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Prism.Navigation;
using ReactiveUI;
using SparkyStudios.TrakLibrary.Common;
using SparkyStudios.TrakLibrary.ViewModel.Common;

namespace SparkyStudios.TrakLibrary.ViewModel
{
    /// <summary>
    /// The <see cref="HomeViewModel"/> is the view model that is associated with the home page view.
    /// Its sole responsibility is to respond to tap events on the page and redirect the user to the desired
    /// page associated with the given buttons. 
    /// </summary>
    public class HomeViewModel : ReactiveViewModel
    {
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        public HomeViewModel(IScheduler scheduler, INavigationService navigationService) : base(scheduler,
            navigationService)
        {
            LoadGamesCommand = ReactiveCommand.CreateFromTask(ExecuteLoadGamesAsync, outputScheduler: scheduler);
        }

        /// <summary>
        /// Command that is invoked by the view when the game button is tapped. When called, the command will propagate
        /// the request and call the <see cref="ExecuteLoadGamesAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadGamesCommand { get; }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoadGamesCommand"/> when activated by the associated view.
        /// This method will merely push the game user entries page to the top of the view stack by swiping it in from
        /// the right..
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task ExecuteLoadGamesAsync()
        {
            await NavigationService.NavigateAsync("GameUserEntriesTabbedPage", new NavigationParameters
            {
                {"transition-type", TransitionType.SlideFromRight}
            });
        }
    }
}