using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class GameInfo : HateoasResource
    {
        public long Id { get; set; }
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public DateTime ReleaseDate { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public AgeRating AgeRating { get; set; }
        
        public long Version { get; set; }
        
        public IEnumerable<Platform> Platforms { get; set; }
        
        public IEnumerable<Publisher> Publishers { get; set; }
        
        public IEnumerable<Genre> Genres { get; set; }
    }
}