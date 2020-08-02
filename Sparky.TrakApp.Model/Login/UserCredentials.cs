using System.Diagnostics.CodeAnalysis;

namespace Sparky.TrakApp.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class UserCredentials
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
    }
}