using System.Threading.Tasks;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Model.Settings;

namespace SparkyStudios.TrakLibrary.Service
{
    public interface IAuthService
    {
        Task<string> GetTokenAsync(UserCredentials userCredentials);
        
        Task<CheckedResponse<bool>> VerifyAsync(string username, string verificationCode);

        Task ReVerifyAsync(string username);

        Task RequestRecoveryAsync(string emailAddress);
        
        Task<CheckedResponse<UserResponse>> RegisterAsync(RegistrationRequest registrationRequest);

        Task<CheckedResponse<UserResponse>> RecoverAsync(RecoveryRequest recoveryRequest);

        Task RequestChangePasswordAsync(string username);

        Task<CheckedResponse<bool>> ChangePasswordAsync(string username, ChangePasswordRequest changePasswordRequest);

        Task<CheckedResponse<bool>> ChangeEmailAddressAsync(string username, ChangeEmailAddressRequest changeEmailAddressRequest);

        Task DeleteByUsernameAsync(string username);
    }
}
