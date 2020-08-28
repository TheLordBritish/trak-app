using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sparky.TrakApp.Service
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(string url);

        Task<T> PostAsync<T>(string url, T requestBody);

        Task<T> PutAsync<T>(string url, T requestBody);

        Task<T> PatchAsync<T>(string url, IDictionary<string, object> values);
        
        Task DeleteAsync(string url);
    }
}