using NUnit.Framework;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace Sparky.TrakApp.UITest.Pages.Login
{
    internal class LoginPage : BasePage
    {
        private readonly Query _usernameEntry;
        private readonly Query _passwordEntry;
        private readonly Query _errorMessageLabel;
        private readonly Query _forgottenPasswordLabel;
        private readonly Query _loginButton;
        private readonly Query _signUpLabel;
        
        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("LoginPage"),
            IOS = x => x.Marked("LoginPage")
        };

        public LoginPage()
        {
            _usernameEntry = x => x.Marked("LoginPageUsernameEntry");
            _passwordEntry = x => x.Marked("LoginPagePasswordEntry");
            _errorMessageLabel = x => x.Marked("LoginPageErrorMessage");
            _forgottenPasswordLabel = x => x.Marked("LoginPageForgottenPasswordLabel");
            _loginButton = x => x.Marked("LoginPageLoginButton");
            _signUpLabel = x => x.Marked("LoginPageSignUpLabel");
        }

        public LoginPage SetUsername(string username)
        {
            App.WaitForElement(_usernameEntry);
            
            App.Tap(_usernameEntry);
            App.ClearText();
            App.EnterText(username);
            App.DismissKeyboard();
            
            return this;
        }
        
        public LoginPage SetPassword(string password)
        {
            App.WaitForElement(_passwordEntry);
            
            App.Tap(_passwordEntry);
            App.ClearText();
            App.EnterText(password);
            App.DismissKeyboard();
            
            return this;
        }

        public LoginPage HasErrorMessage(string errorMessage)
        {
            App.WaitForElement(_errorMessageLabel);
            Assert.AreEqual(App.Query(_errorMessageLabel)[0].Text, errorMessage);

            return this;
        }
        
        public void TapForgottenPassword()
        {
            App.WaitForElement(_forgottenPasswordLabel);
            App.Tap(_forgottenPasswordLabel);
        }

        public void TapLogin()
        {
            App.WaitForElement(_loginButton);
            App.Tap(_loginButton);
        }

        public void TapSignUp()
        {
            App.WaitForElement(_signUpLabel);
            App.Tap(_signUpLabel);
        }
    }
}