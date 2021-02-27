using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class GameUserEntryDownloadableContent
    {
        public long Id { get; set; }
        
        public long GameUserEntryId { get; set; }
        
        public long DownloadableContentId { get; set; }
        
        public string downloadableContentName { get; set; }
        
        public long? Version { get; set; }
    }
}