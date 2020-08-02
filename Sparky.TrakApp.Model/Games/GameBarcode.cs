using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Model.Games
{
    [ExcludeFromCodeCoverage]
    public class GameBarcode : HateoasResource
    {
        public long Id { get; set; }
        
        public long GameId { get; set; }
        
        public long PlatformId { get; set; }
        
        public string Barcode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BarcodeType BarcodeType { get; set; }
        
        public long Version { get; set; }
    }
}