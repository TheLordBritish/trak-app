using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sparky.TrakApp.Service
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(string url, string authToken);

        Task<T> PostAsync<T>(string url, T requestBody, string authToken);

        Task<T> PutAsync<T>(string url, T requestBody, string authToken);

        Task<T> PatchAsync<T>(string url, IDictionary<string, object> values, string authToken);
        
        Task DeleteAsync(string url, string authToken);
    }
}