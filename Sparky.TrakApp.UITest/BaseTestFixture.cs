using NUnit.Framework;
using Xamarin.UITest;

namespace Sparky.TrakApp.UITest
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class BaseTestFixture
    {
        protected IApp App => AppManager.App;
        protected bool OnAndroid => AppManager.Platform == Platform.Android;
        protected bool OnIOS => AppManager.Platform == Platform.iOS;
        
        public BaseTestFixture(Platform platform)
        {
            AppManager.Platform = platform;
        }

        [SetUp]
        public virtual void BeforeEachTest()
        {
            AppManager.StartApp();
        }
    }
}