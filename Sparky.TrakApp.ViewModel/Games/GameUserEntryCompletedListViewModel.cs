using Acr.UserDialogs;
using Prism.Navigation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Service;

namespace Sparky.TrakApp.ViewModel.Games
{
    public class GameUserEntryCompletedListViewModel : GameUserEntryListViewModel
    {
        public GameUserEntryCompletedListViewModel(INavigationService navigationService, IStorageService storageService, IUserDialogs userDialogs, IRestService restService) 
            : base(navigationService, storageService, userDialogs, restService, GameUserEntryStatus.Completed)
        {
            
        }
    }
}