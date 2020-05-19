using System.Threading.Tasks;

namespace Sparky.TrakApp.Service
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(string url, string authToken);
    }
}