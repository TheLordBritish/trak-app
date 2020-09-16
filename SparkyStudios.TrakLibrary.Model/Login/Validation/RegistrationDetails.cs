using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Login.Validation
{
    [ExcludeFromCodeCoverage]
    public class RegistrationDetails
    {
        public string Username { get; set; }
        
        public string EmailAddress { get; set; }
        
        public string Password { get; set; }
        
        public string ConfirmPassword { get; set; }
    }
}