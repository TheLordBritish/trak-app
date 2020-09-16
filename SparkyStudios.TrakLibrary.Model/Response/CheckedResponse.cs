using System.Diagnostics.CodeAnalysis;

namespace SparkyStudios.TrakLibrary.Model.Response
{
    [ExcludeFromCodeCoverage]
    public class CheckedResponse<T>
    {
        public T Data { get; set; }
        
        public bool Error { get; set; }
        
        public string ErrorMessage { get; set; }
    }
}