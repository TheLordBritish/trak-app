using System.Net;

namespace SparkyStudios.TrakLibrary.Service.Exception
{
    public class ApiException : System.Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        
        public string Content { get; set; }
    }
}