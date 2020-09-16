using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class NotificationRegistrationRequest
    {
        public long UserId { get; set; }
        
        public string DeviceGuid { get; set; }
        
        public string Token { get; set; }
    }
}