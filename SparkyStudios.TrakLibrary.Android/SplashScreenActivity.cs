using Android.App;
using Android.Content.PM;

namespace SparkyStudios.TrakLibrary.Droid
{
    [Activity(Label = "Trak",  Icon = "@mipmap/ic_launcher", Theme = "@style/SplashScreen", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
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