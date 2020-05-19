namespace Sparky.TrakApp.Model.Login
{
    public class UserResponse
    {
        public long Id { get; set; }
        
        public string Username { get; set; }
        
        public bool Verified { get; set; }
    }
}