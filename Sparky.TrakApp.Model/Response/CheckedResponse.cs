namespace Sparky.TrakApp.Model.Response
{
    public class CheckedResponse<T>
    {
        public T Data { get; set; }
        
        public bool Error { get; set; }
        
        public string ErrorMessage { get; set; }
    }
}