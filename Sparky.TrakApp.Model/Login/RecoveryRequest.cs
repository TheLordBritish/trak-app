using System.Diagnostics.CodeAnalysis;

namespace Sparky.TrakApp.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class RecoveryRequest
    {
        public string Username { get; set; }
        
        public string RecoveryToken { get; set; }
        
        public string Password { get; set; }
    }
}