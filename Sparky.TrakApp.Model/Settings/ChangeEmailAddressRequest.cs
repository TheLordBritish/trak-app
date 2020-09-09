using System.Diagnostics.CodeAnalysis;

namespace Sparky.TrakApp.Model.Settings
{
    [ExcludeFromCodeCoverage]
    public class ChangeEmailAddressRequest
    {
        public string EmailAddress { get; set; }
    }
}