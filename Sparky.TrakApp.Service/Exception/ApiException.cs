using System.Net;

namespace Sparky.TrakApp.Service.Exception
{
    public class ApiException : System.Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        
        public string Content { get; set; }
    }
}