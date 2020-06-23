using System.Threading.Tasks;

namespace Sparky.TrakApp.Service
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(string url, string authToken);

        Task<T> PostAsync<T>(string url, T requestBody, string authToken);

        Task DeleteAsync(string url, string authToken);
    }
}