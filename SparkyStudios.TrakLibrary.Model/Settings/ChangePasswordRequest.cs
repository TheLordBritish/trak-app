using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Settings
{
    [ExcludeFromCodeCoverage]
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}