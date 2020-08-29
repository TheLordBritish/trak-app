using System.Diagnostics.CodeAnalysis;

namespace Sparky.TrakApp.Model.Settings
{
    [ExcludeFromCodeCoverage]
    public class ChangePasswordRequest
    {
        public string RecoveryToken { get; set; }
        
        public string Username { get; set; }

        public string NewPassword { get; set; }
    }
}