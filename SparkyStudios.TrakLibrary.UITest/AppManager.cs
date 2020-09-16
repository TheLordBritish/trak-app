using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace SparkyStudios.TrakLibrary.UITest
{
    public class AppManager
    {
        private static IApp _app;
        private static Platform? _platform;

        public static IApp App
        {
            get
            {
                if (_app == null)
                {
                    throw new NullReferenceException("'AppManager.App' not set. Call 'AppManager.StartApp()' before trying to access it.");
                }
                return _app;
            }
        }
        
        public static Platform Platform
        {
            get
            {
                if (_platform == null)
                {
                    throw new NullReferenceException("'AppManager.Platform' not set.");
                }
                return _platform.Value;
            }
            set => _platform = value;
        }
        
        public static void StartApp()
        {
            switch (Platform)
            {
                case Platform.Android:
                    _app = ConfigureApp
                        .Android
                        .EnableLocalScreenshots()
                        .PreferIdeSettings()
                        .InstalledApp("com.sparkystudios.traklibrary")
                        .StartApp(Xamarin.UITest.Configuration.AppDataMode.DoNotClear);
                    break;

                case Platform.iOS:
                    _app = ConfigureApp.iOS.StartApp();
                    break;
                
                default:
                    throw new ArgumentException("Unknown platform");
            }
        }
    }
}