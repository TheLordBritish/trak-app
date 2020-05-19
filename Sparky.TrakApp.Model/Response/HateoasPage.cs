using Newtonsoft.Json;

namespace Sparky.TrakApp.Model.Response
{
    public class HateoasPage<T> : HateoasResource where T : HateoasResource
    {
        [JsonProperty("_embedded")]
        public HateoasResourceCollection<T> Embedded { get; set; }
        
        [JsonProperty("page")]
        public HateoasPageData PageData { get; set; }

        public bool HasNext => Links != null && Links.ContainsKey("next");

        public bool HasPrevious => Links != null && Links.ContainsKey("prev");
    }
}