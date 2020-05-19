using System.Collections.Generic;

namespace Sparky.TrakApp.Model.Response
{
    public class HateoasResourceCollection<T> where T : HateoasResource
    {
        public IEnumerable<T> Data { get; set; }
    }
}