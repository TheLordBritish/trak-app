using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Games.Validation
{
    [ExcludeFromCodeCoverage]
    public class GameRequestDetails
    {
        public string Title { get; set; }
        
        public string Notes { get; set; }
    }
}