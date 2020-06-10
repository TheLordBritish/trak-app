using System.Threading.Tasks;

namespace Sparky.TrakApp.Service
{
    public interface IStorageService
    {
        Task<long> GetUserIdAsync();

        Task SetUserIdAsync(long userId);
        
        Task<string> GetUsernameAsync();

        Task SetUsernameAsync(string username);

        Task<string> GetPasswordAsync();

        Task SetPasswordAsync(string password);
        
        Task<string> GetAuthTokenAsync();

        Task SetAuthTokenAsync(string authToken);
    }
}