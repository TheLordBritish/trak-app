using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class LoginRequest
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
    }
}