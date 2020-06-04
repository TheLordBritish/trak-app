using Newtonsoft.Json;

namespace Sparky.TrakApp.Model.Response
{
    public class HateoasPage<T> : HateoasCollection<T> where T : HateoasResource
    {
        [JsonProperty("page")]
        public HateoasPageData PageData { get; set; }

        public bool HasNext => Links != null && Links.ContainsKey("next");

        public bool HasPrevious => Links != null && Links.ContainsKey("prev");
    }
}