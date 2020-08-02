using System.Diagnostics.CodeAnalysis;

namespace Sparky.TrakApp.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class NotificationRegistrationRequest
    {
        public long UserId { get; set; }
        
        public string DeviceGuid { get; set; }
        
        public string Token { get; set; }
    }
}