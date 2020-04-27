using System.Threading.Tasks;
using Sparky.TrakApp.Service;
using Xamarin.Essentials;

namespace Sparky.TrakApp.ViewModel.Impl
{
    public class SecureStorageService : IStorageService
    {
        public async Task<string> GetUsernameAsync()
        {
            return await SecureStorage.GetAsync("username");
        }

        public async Task SetUsernameAsync(string username)
        {
            await SecureStorage.SetAsync("username", username);
        }

        public async Task<string> GetAuthTokenAsync()
        {
            return await SecureStorage.GetAsync("auth-token");
        }

        public async Task SetAuthTokenAsync(string authToken)
        {
            await SecureStorage.SetAsync("auth-token", authToken);
        }
    }
}