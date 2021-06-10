using System.Threading.Tasks;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Model.Settings;

namespace SparkyStudios.TrakLibrary.Service
{
    public interface IAuthService
    {
        Task<string> GetTokenAsync(LoginRequest userCredentials);
        
        Task<CheckedResponse<bool>> VerifyAsync(long userId, string verificationCode);

        Task ReVerifyAsync(long userId);

        Task RequestRecoveryAsync(string emailAddress);
        
        Task<CheckedResponse<RegistrationResponse>> RegisterAsync(RegistrationRequest registrationRequest);

        Task<CheckedResponse<UserResponse>> RecoverAsync(RecoveryRequest recoveryRequest);
        
        Task<CheckedResponse<bool>> ChangePasswordAsync(long userId, ChangePasswordRequest changePasswordRequest);

        Task<CheckedResponse<bool>> ChangeEmailAddressAsync(long userId, ChangeEmailAddressRequest changeEmailAddressRequest);

        Task DeleteByIdAsync(long userId);
    }
}
