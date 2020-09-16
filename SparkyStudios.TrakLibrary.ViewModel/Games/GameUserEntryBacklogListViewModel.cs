using System.Reactive.Concurrency;
using Acr.UserDialogs;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Service;

namespace SparkyStudios.TrakLibrary.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameUserEntryBacklogListViewModel"/> is the view model that is associated with a backlog game user entry
    /// list page view. Its responsibility is to display the users personal collection of games within the backlog status.
    /// </summary>
    public class GameUserEntryBacklogListViewModel : GameUserEntryListViewModel
    {
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler"/> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> instance to inject.</param>
        /// <param name="userDialogs">The <see cref="IUserDialogs"/> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService"/> instance to inject.</param>
        public GameUserEntryBacklogListViewModel(IScheduler scheduler, INavigationService navigationService,
            IStorageService storageService, IUserDialogs userDialogs, IRestService restService)
            : base(scheduler, navigationService, storageService, userDialogs, restService, GameUserEntryStatus.Backlog)
        {
        }
    }
}