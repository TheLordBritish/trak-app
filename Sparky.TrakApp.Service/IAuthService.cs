using System.Threading.Tasks;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.Model.Response;

namespace Sparky.TrakApp.Service
{
    public interface IAuthService
    {
        Task<string> GetTokenAsync(UserCredentials userCredentials);
        
        Task<CheckedResponse<bool>> VerifyAsync(string username, string verificationCode);

        Task ReVerifyAsync(string username);

        Task RequestRecoveryAsync(string emailAddress);
        
        Task<CheckedResponse<UserResponse>> RegisterAsync(RegistrationRequest registrationRequest);

        Task<CheckedResponse<UserResponse>> RecoverAsync(RecoveryRequest recoveryRequest);
    }
}