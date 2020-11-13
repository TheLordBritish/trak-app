using System.Collections.Generic;

namespace SparkyStudios.TrakLibrary.Model.Games.Request
{
    public class GameUserEntryRequest
    {
        public long GameUserEntryId { get; set; }
        
        public long UserId { get; set; }
        
        public long GameId { get; set; }
        
        public short Rating { get; set; }
        
        public GameUserEntryStatus Status { get; set; }
        
        public IEnumerable<long> PlatformIds { get; set; }
    }
}