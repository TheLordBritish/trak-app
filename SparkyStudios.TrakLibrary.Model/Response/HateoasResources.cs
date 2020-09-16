using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Response
{
    [ExcludeFromCodeCoverage]
    public class HateoasResources<T> where T : HateoasResource
    {
        public IEnumerable<T> Data { get; set; }
    }
}