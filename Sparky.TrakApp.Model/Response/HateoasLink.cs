using System;
using Newtonsoft.Json;
using Sparky.TrakApp.Common.Converters;

namespace Sparky.TrakApp.Model.Response
{
    public class HateoasLink
    {
        [JsonConverter(typeof(UriConverter))]
        public Uri Href { get; set; }
    }
}