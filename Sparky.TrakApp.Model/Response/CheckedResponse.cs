using System.Diagnostics.CodeAnalysis;

namespace Sparky.TrakApp.Model.Response
{
    [ExcludeFromCodeCoverage]
    public class CheckedResponse<T>
    {
        public T Data { get; set; }
        
        public bool Error { get; set; }
        
        public string ErrorMessage { get; set; }
    }
}