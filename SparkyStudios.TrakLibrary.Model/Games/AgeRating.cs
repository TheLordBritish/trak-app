using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SparkyStudios.TrakLibrary.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class AgeRating
    {
        public long Id { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public AgeRatingClassification Classification { get; set; }
        
        public short Rating { get; set; }
        
        public long? Version { get; set; }
    }
}