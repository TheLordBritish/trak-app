using Android.App;

namespace Sparky.TrakApp.Droid
{
    [Activity(Label = "Trak",  Icon = "@mipmap/icon", Theme = "@style/SplashScreen", MainLauncher = true, NoHistory = true)]
    public class SplashScreenActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(new Android.Content.Intent(Application.Context, typeof(MainActivity)));
        }

        public override void OnBackPressed()
        {
            // Empty.
        }
    }
}