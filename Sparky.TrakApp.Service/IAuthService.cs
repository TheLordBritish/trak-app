using System.Threading.Tasks;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Service
{
    public interface IAuthService
    {
        Task<string> GetTokenAsync(UserCredentials userCredentials);

        Task<UserResponse> GetFromUsernameAsync(string username, string authToken);

        Task VerifyAsync(string username, short verificationCode, string authToken);

        Task<CheckedResponse<UserResponse>> RegisterAsync(RegistrationRequest registrationRequest);
    }
}