using System;
using System.Linq;
using Prism.Navigation;
using Sparky.TrakApp.Model.Games;

namespace Sparky.TrakApp.ViewModel.Games
{
    public class GameUserEntryViewModel : BaseViewModel
    {
        private readonly GameUserEntry _gameUserEntry;
        
        public GameUserEntryViewModel(INavigationService navigationService, GameUserEntry gameUserEntry) : base(navigationService)
        {
            _gameUserEntry = gameUserEntry;
        }

        public long Id => _gameUserEntry.Id;

        public long GameId => _gameUserEntry.GameId;

        public string GameTitle => _gameUserEntry.GameTitle;

        public DateTime GameReleaseDate => _gameUserEntry.GameReleaseDate;

        public string PlatformName => _gameUserEntry.PlatformName;
        
        public string Publishers => string.Join(", ", _gameUserEntry.Publishers.OrderBy(s => s));

        public short Rating => _gameUserEntry.Rating;

        public Uri ImageUrl => _gameUserEntry.GetLink("image");
    }
}