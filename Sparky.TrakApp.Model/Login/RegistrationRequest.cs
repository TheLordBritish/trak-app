using System.Diagnostics.CodeAnalysis;

namespace Sparky.TrakApp.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class RegistrationRequest
    {
        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }
    }
}
