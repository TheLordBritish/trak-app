using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class GameUserEntryPlatform
    {
        public long Id { get; set; }
        
        public long GameUserEntryId { get; set; }
        
        public long PlatformId { get; set; }
        
        public string PlatformName { get; set; }
        
        public long Version { get; set; }
    }
}