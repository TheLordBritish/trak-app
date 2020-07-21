using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Model.Games
{
    public class GameUserEntry : HateoasResource
    {
        public long Id { get; set; }
        
        public long GameId { get; set; }
        
        public string GameTitle { get; set; }
        
        public DateTime? GameReleaseDate { get; set; }
        
        public long PlatformId { get; set; }
        
        public string PlatformName { get; set; }
        
        public long UserId { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public GameUserEntryStatus Status { get; set; }
        
        public IEnumerable<string> Publishers { get; set; }
        
        public short Rating { get; set; }
        
        public long Version { get; set; }
    }
}