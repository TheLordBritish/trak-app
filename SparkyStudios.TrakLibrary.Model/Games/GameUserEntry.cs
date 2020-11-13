using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SparkyStudios.TrakLibrary.Model.Response;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class GameUserEntry : HateoasResource
    {
        public long Id { get; set; }
        
        public long GameId { get; set; }
        
        public string GameTitle { get; set; }
        
        public long UserId { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public GameUserEntryStatus Status { get; set; }
        
        public IEnumerable<string> Publishers { get; set; }
        
        public short Rating { get; set; }
        
        public IEnumerable<GameUserEntryPlatform> GameUserEntryPlatforms { get; set; }
        
        public long Version { get; set; }
    }
}