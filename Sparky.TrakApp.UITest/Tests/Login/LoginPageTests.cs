using NUnit.Framework;
using Sparky.TrakApp.UITest.Pages.Login;
using Xamarin.UITest;

namespace Sparky.TrakApp.UITest.Tests.Login
{
    public class LoginPageTests : BaseTestFixture
    {
        public LoginPageTests(Platform platform) : base(platform)
        {
        }

        [Test]
        public void LoginPage_Appears()
        {
            new LoginPage();
        }

        [Test]
        public void LoginPage_WithEmptyCredentials_DisplaysValidationErrorMessages()
        {
            new LoginPage()
                .TapLogin();

            new LoginPage()
                .HasUsernameErrorMessage("No username has been entered.")
                .HasPasswordErrorMessage("No password has been entered.");
        }
        
        [Test]
        public void LoginPage_WithIncorrectCredentials_DisplaysUnauthorisedErrorMessage()
        {
            new LoginPage()
                .SetUsername("NotAUser")
                .SetPassword("Password123")
                .TapLogin();

            new LoginPage()
                .HasErrorMessage("Your credentials were incorrect. Please try again.");
        }
    }
}