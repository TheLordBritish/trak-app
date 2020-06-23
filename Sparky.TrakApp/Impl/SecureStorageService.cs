using System;
using System.Threading.Tasks;
using Sparky.TrakApp.Service;
using Xamarin.Essentials;

namespace Sparky.TrakApp.Impl
{
    public class SecureStorageService : IStorageService
    {
        public async Task<long> GetUserIdAsync()
        {
            return long.Parse(await SecureStorage.GetAsync("userId"));
        }

        public async Task SetUserIdAsync(long userId)
        {
            await SecureStorage.SetAsync("userId", userId.ToString());
        }

        public async Task<string> GetUsernameAsync()
        {
            return await SecureStorage.GetAsync("username");
        }

        public async Task SetUsernameAsync(string username)
        {
            await SecureStorage.SetAsync("username", username);
        }

        public async Task<string> GetPasswordAsync()
        {
            return await SecureStorage.GetAsync("password");
        }

        public async Task SetPasswordAsync(string password)
        {
            await SecureStorage.SetAsync("password", password);
        }

        public async Task<string> GetAuthTokenAsync()
        {
            return await SecureStorage.GetAsync("auth-token");
        }

        public async Task SetAuthTokenAsync(string authToken)
        {
            await SecureStorage.SetAsync("auth-token", authToken);
        }

        public async Task<string> GetNotificationTokenAsync()
        {
            return await SecureStorage.GetAsync("notification-token");
        }

        public async Task SetNotificationTokenAsync(string notificationToken)
        {
            await SecureStorage.SetAsync("notification-token", notificationToken);
        }

        public async Task<Guid> GetDeviceIdAsync()
        {
            var deviceId = await SecureStorage.GetAsync("device-id");
            return !string.IsNullOrEmpty(deviceId) ? Guid.Parse(deviceId) : Guid.Empty;
        }

        public async Task SetDeviceIdAsync(Guid deviceId)
        {
            await SecureStorage.SetAsync("device-id", deviceId.ToString());
        }
    }
}