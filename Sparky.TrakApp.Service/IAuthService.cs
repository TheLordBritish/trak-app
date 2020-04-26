using System.Threading.Tasks;
using Sparky.TrakApp.Model.Login;

namespace Sparky.TrakApp.Service
{
    public interface IAuthService
    {
        Task<string> GetTokenAsync(UserCredentials userCredentials);

        Task<bool> IsVerifiedAsync(string username, string authToken);

        Task VerifyAsync(string username, short verificationCode, string authToken);
    }
}