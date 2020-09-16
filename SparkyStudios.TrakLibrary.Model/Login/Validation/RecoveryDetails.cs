using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Login.Validation
{
    [ExcludeFromCodeCoverage]
    public class RecoveryDetails
    {
        public string Username { get; set; }
        
        public string RecoveryToken { get; set; }
        
        public string Password { get; set; }
        
        public string ConfirmPassword { get; set; }
    }
}