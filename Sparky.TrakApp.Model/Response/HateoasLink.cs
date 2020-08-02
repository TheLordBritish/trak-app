using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Sparky.TrakApp.Common.Converters;

namespace Sparky.TrakApp.Model.Response
{
    [ExcludeFromCodeCoverage]
    public class HateoasLink
    {
        [JsonConverter(typeof(UriConverter))]
        public Uri Href { get; set; }
    }
}