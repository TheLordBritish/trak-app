using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Settings.Validation
{
    [ExcludeFromCodeCoverage]
    public class ChangePasswordDetails
    {
        public string CurrentPassword { get; set; }
        
        public string NewPassword { get; set; }
        
        public string ConfirmNewPassword { get; set; }
    }
}