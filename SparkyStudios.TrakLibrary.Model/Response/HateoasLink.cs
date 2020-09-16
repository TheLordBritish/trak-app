using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using SparkyStudios.TrakLibrary.Common.Converters;

namespace SparkyStudios.TrakLibrary.Model.Response
{
    [ExcludeFromCodeCoverage]
    public class HateoasLink
    {
        [JsonConverter(typeof(UriConverter))]
        public Uri Href { get; set; }
    }
}