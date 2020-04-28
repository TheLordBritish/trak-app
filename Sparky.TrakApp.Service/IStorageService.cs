using System.Threading.Tasks;

namespace Sparky.TrakApp.Service
{
    public interface IStorageService
    {
        Task<string> GetUsernameAsync();

        Task SetUsernameAsync(string username);
        
        Task<string> GetAuthTokenAsync();

        Task SetAuthTokenAsync(string authToken);
    }
}