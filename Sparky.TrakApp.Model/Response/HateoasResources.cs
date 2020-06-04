using System.Collections.Generic;

namespace Sparky.TrakApp.Model.Response
{
    public class HateoasResources<T> where T : HateoasResource
    {
        public IEnumerable<T> Data { get; set; }
    }
}