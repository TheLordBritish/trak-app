using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Firebase.Messaging;
using Prism.DryIoc;
using Sparky.TrakApp.Service;

namespace Sparky.TrakApp.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT"})]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            
            // Retrieve the DI container and the storage service so that we can store the firebase token.
            var container = ((PrismApplication) Prism.PrismApplicationBase.Current).Container;
            
            // Retrieve storage service and set the notification token as the firebase token.
            var storageService = container.Resolve(typeof(IStorageService)) as IStorageService;
            storageService?.SetNotificationTokenAsync(token);
        }

        public override void OnMessageReceived(RemoteMessage remoteMessage)
        {
            base.OnMessageReceived(remoteMessage);
         
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);

            var pendingIntent =
                PendingIntent.GetActivity(this, MainActivity.NOTIFICATION_ID, intent, PendingIntentFlags.OneShot);
            
            var notification = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                .SetContentTitle("Trak")
                .SetDefaults((int) (NotificationDefaults.Sound | NotificationDefaults.Vibrate))
                .SetContentText(remoteMessage.Data.ContainsKey("body") ? remoteMessage.Data["body"] : remoteMessage.Data["default"])
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent)
                .SetSmallIcon(Resource.Drawable.logo_background)
                .Build();

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, notification);
        }
    }
}