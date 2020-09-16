using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Response
{
    [ExcludeFromCodeCoverage]
    public class HateoasPageData
    {
        public int Size { get; set; }
        
        public int TotalElements { get; set; }
        
        public int TotalPages { get; set; }
        
        public int Number { get; set; }
    }
}