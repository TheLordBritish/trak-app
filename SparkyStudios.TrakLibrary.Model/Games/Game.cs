using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SparkyStudios.TrakLibrary.Model.Response;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class Game : HateoasResource
    {
        public long Id { get; set; }
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public DateTime ReleaseDate { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public AgeRating AgeRating { get; set; }
        
        public long Version { get; set; }
    }
}