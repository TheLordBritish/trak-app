using System.Reactive.Concurrency;
using Prism.Navigation;
using ReactiveUI.Fody.Helpers;
using Sparky.TrakApp.ViewModel.Common;

namespace Sparky.TrakApp.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameUserEntriesTabbedViewModel"/> is the view model that is associated with the game user entries tabbed
    /// page view. Its responsibility is to define different view models that are used by each individual tab within the page,
    /// which are backlog, playing, done and dropped.
    /// </summary>
    public class GameUserEntriesTabbedViewModel : ReactiveViewModel
    {
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="backlogViewModel">The <see cref="GameUserEntryBacklogListViewModel"/> instance to inject.</param>
        /// /// <param name="inProgressViewModel">The <see cref="GameUserEntryInProgressListViewModel"/> instance to inject.</param>
        /// /// <param name="completedViewModel">The <see cref="GameUserEntryCompletedListViewModel"/> instance to inject.</param>
        /// /// <param name="droppedViewModel">The <see cref="GameUserEntryDroppedListViewModel"/> instance to inject.</param>
        public GameUserEntriesTabbedViewModel(IScheduler scheduler, INavigationService navigationService, 
            GameUserEntryBacklogListViewModel backlogViewModel, 
            GameUserEntryInProgressListViewModel inProgressViewModel,
            GameUserEntryCompletedListViewModel completedViewModel,
            GameUserEntryDroppedListViewModel droppedViewModel) : base(scheduler, navigationService)
        {
            BacklogViewModel = backlogViewModel;
            InProgressViewModel = inProgressViewModel;
            CompletedViewModel = completedViewModel;
            DroppedViewModel = droppedViewModel;
        }

        /// <summary>
        /// A <see cref="GameUserEntryBacklogListViewModel"/> that is used for the backlog tab page.
        /// </summary>
        [Reactive]
        public GameUserEntryBacklogListViewModel BacklogViewModel { get; private set; }

        /// <summary>
        /// A <see cref="GameUserEntryInProgressListViewModel"/> that is used for the in progress tab page.
        /// </summary>
        [Reactive]
        public GameUserEntryInProgressListViewModel InProgressViewModel { get; private set; }

        /// <summary>
        /// A <see cref="GameUserEntryCompletedListViewModel"/> that is used for the done tab page.
        /// </summary>
        [Reactive]
        public GameUserEntryCompletedListViewModel CompletedViewModel { get; private set; }

        /// <summary>
        /// A <see cref="GameUserEntryDroppedListViewModel"/> that is used for the dropped tab page.
        /// </summary>
        [Reactive]
        public GameUserEntryDroppedListViewModel DroppedViewModel { get; private set; }
    }
}