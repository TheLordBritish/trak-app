using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Model.Games
{
    public class GamePlatformXref : HateoasResource
    {
        public long Id { get; set; }
        
        public long GameId { get; set; }
        
        public long PlatformId { get; set; }
        
        public string UpcABarcode { get; set; }
        
        public string Ean13Barcode { get; set; }
        
        public long Version { get; set; }
    }
}