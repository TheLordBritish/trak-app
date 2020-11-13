using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Runtime;
using Android.OS;
using Prism;
using Prism.Ioc;

namespace SparkyStudios.TrakLibrary.Droid
{
    [Activity(Label = "Trak", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            UserDialogs.Init(this);
            
            // Set the default styles for popups used by the ACR User Dialogs library.
            const int dialogStyle = Resource.Style.AppCompatDialogStyle;
            AlertConfig.DefaultAndroidStyleId = dialogStyle;
            PromptConfig.DefaultAndroidStyleId = dialogStyle;
            ConfirmConfig.DefaultAndroidStyleId = dialogStyle;
            ActionSheetConfig.DefaultAndroidStyleId = dialogStyle;
            DatePromptConfig.DefaultAndroidStyleId = dialogStyle;
            TimePromptConfig.DefaultAndroidStyleId = dialogStyle;
            LoginConfig.DefaultAndroidStyleId = dialogStyle;
            
            // We only want to generate a notification channel for push notifications if google play is available
            // on the device running it. 
            if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this) == ConnectionResult.Success)
            {
                CreateNotificationChannel();
            }
            
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification 
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID, "FCM Notifications", NotificationImportance.Default)
            {
                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            notificationManager?.CreateNotificationChannel(channel);
        }
    }
    
    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }
}