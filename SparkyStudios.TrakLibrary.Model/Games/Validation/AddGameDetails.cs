using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Games.Validation
{
    [ExcludeFromCodeCoverage]
    public class AddGameDetails
    {
        public Platform Platform { get; set; }
        
        public GameUserEntryStatus Status { get; set; }
    }
}