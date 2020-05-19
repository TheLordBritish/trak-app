using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace Sparky.TrakApp.UITest.Pages.Login
{
    internal class RegisterPage : BasePage
    {
        private readonly Query _usernameEntry;
        
        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("RegisterPage"),
            IOS = x => x.Marked("RegisterPage")
        };

        public RegisterPage()
        {
            _usernameEntry = x => x.Marked("RegisterPageUsernameEntry");
        }
        
        public RegisterPage SetUsername(string username)
        {
            App.WaitForElement(_usernameEntry);
            App.ScrollDownTo(_usernameEntry);
            
            App.Tap(_usernameEntry);
            App.ClearText();
            App.EnterText(username);
            App.DismissKeyboard();
            
            return this;
        }
    }
}