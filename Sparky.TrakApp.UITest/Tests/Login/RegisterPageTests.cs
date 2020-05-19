using NUnit.Framework;
using Sparky.TrakApp.UITest.Pages.Login;
using Xamarin.UITest;

namespace Sparky.TrakApp.UITest.Tests.Login
{
    public class RegisterPageTests : BaseTestFixture
    {
        public RegisterPageTests(Platform platform) : base(platform)
        {
        }

        [Test]
        public void CanGetToRegisterPage()
        {
            new LoginPage()
                .TapSignUp();

            new RegisterPage();
        }
    }
}