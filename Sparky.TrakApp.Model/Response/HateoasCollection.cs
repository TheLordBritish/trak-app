using Newtonsoft.Json;

namespace Sparky.TrakApp.Model.Response
{
    public class HateoasCollection<T> : HateoasResource where T : HateoasResource
    {
        [JsonProperty("_embedded")]
        public HateoasResources<T> Embedded { get; set; }
    }
}