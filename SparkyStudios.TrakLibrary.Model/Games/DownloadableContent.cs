using System;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    public class DownloadableContent
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public DateTime ReleaseDate { get; set; }
        
        public long? Version { get; set; }
    }
}