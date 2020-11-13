using System;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public class GameReleaseDate
    {
        public long Id { get; set; }
        
        public GameRegion Region { get; set; }
        
        public DateTime ReleaseDate { get; set; }
        
        public long? Version { get; set; }
    }
}