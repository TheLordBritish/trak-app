using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SparkyStudios.TrakLibrary.Model.Response;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class GameUserEntryFilters : HateoasResource
    {
        public IEnumerable<GameFilter> Platforms { get; set; }
        
        public IEnumerable<GameFilter> Genres { get; set; }
        
        public IEnumerable<GameMode> GameModes { get; set; }
        
        public IEnumerable<AgeRating> AgeRatings { get; set; }

        public IEnumerable<GameUserEntryStatus> Statuses { get; set; }
    }
}