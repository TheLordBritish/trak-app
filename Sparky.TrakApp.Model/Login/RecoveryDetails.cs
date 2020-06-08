namespace Sparky.TrakApp.Model.Login
{
    public class RecoveryDetails
    {
        public string Username { get; set; }
        
        public string RecoveryToken { get; set; }
        
        public string Password { get; set; }
        
        public string ConfirmPassword { get; set; }
    }
}