using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class UserCredentials
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
    }
}