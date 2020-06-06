using System.Threading.Tasks;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Service
{
    public interface IAuthService
    {
        Task<string> GetTokenAsync(UserCredentials userCredentials);

        Task<UserResponse> GetFromUsernameAsync(string username, string authToken);

        Task<CheckedResponse<bool>> VerifyAsync(string username, string verificationCode, string authToken);

        Task ReVerifyAsync(string username, string authToken);

        Task ResetPasswordAsync(string emailAddress);
        
        Task<CheckedResponse<UserResponse>> RegisterAsync(RegistrationRequest registrationRequest);
    }
}