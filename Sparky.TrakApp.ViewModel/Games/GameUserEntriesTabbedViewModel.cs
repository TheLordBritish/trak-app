using Prism.Navigation;

namespace Sparky.TrakApp.ViewModel.Games
{
    public class GameUserEntriesTabbedViewModel : BaseViewModel
    {
        private GameUserEntryBacklogListViewModel _backlogViewModel;
        private GameUserEntryInProgressListViewModel _inProgressViewModel;
        private GameUserEntryCompletedListViewModel _completedViewModel;
        private GameUserEntryDroppedListViewModel _droppedViewModel;
        
        public GameUserEntriesTabbedViewModel(INavigationService navigationService, 
            GameUserEntryBacklogListViewModel backlogViewModel, 
            GameUserEntryInProgressListViewModel inProgressViewModel,
            GameUserEntryCompletedListViewModel completedViewModel,
            GameUserEntryDroppedListViewModel droppedViewModel) : base(navigationService)
        {
            BacklogViewModel = backlogViewModel;
            InProgressViewModel = inProgressViewModel;
            CompletedViewModel = completedViewModel;
            DroppedViewModel = droppedViewModel;
        }

        public GameUserEntryBacklogListViewModel BacklogViewModel
        {
            get => _backlogViewModel;
            set => SetProperty(ref _backlogViewModel, value);
        }

        public GameUserEntryInProgressListViewModel InProgressViewModel
        {
            get => _inProgressViewModel;
            set => SetProperty(ref _inProgressViewModel, value);
        }
        
        public GameUserEntryCompletedListViewModel CompletedViewModel
        {
            get => _completedViewModel;
            set => SetProperty(ref _completedViewModel, value);
        }

        public GameUserEntryDroppedListViewModel DroppedViewModel
        {
            get => _droppedViewModel;
            set => SetProperty(ref _droppedViewModel, value);
        }
    }
}