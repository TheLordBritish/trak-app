using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Login
{
    [ExcludeFromCodeCoverage]
    public class UserResponse
    {
        public long Id { get; set; }
        
        public string Username { get; set; }
        
        public bool Verified { get; set; }
    }
}