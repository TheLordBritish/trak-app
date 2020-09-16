using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace SparkyStudios.TrakLibrary.Model.Response
{
    public class HateoasPage<T> : HateoasCollection<T> where T : HateoasResource
    {
        [ExcludeFromCodeCoverage]
        [JsonProperty("page")]
        public HateoasPageData PageData { get; set; }

        public bool HasNext => Links != null && Links.ContainsKey("next");

        public bool HasPrevious => Links != null && Links.ContainsKey("prev");
    }
}