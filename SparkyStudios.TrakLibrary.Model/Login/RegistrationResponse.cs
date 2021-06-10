using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class RegistrationResponse
    {
        public long UserId { get; set; }
        
        public byte[] QrData { get; set; }
    }
}