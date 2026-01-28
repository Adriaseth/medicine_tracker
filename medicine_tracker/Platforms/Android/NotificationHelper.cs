using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;

namespace medicine_tracker.Platforms.Android
{
	public static class NotificationHelper
	{
		// Bump channel id when changing channel settings (Android won't apply changes to an existing channel).
		const string CHANNEL_ID = "reminder_channel_v2";
		const string CHANNEL_SOUND_NAME = "bottle_shake";

		public static void ShowNotification(Context context, int reminderId, string? reminderName = null)
		{
			CreateChannel(context);
			var name = string.IsNullOrWhiteSpace(reminderName) ? "Reminder" : reminderName;

			var takenIntent = new Intent(context, typeof(Receivers.NotificationActionReceiver));
			takenIntent.SetAction(Receivers.NotificationActionReceiver.ActionTaken);
			takenIntent.PutExtra(Services.AlarmScheduler.ExtraReminderId, reminderId);
			var takenPendingIntent = PendingIntent.GetBroadcast(
				context,
				reminderId,
				takenIntent,
				PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

			var builder = new NotificationCompat.Builder(context, CHANNEL_ID)
				.SetContentTitle("Reminder")
				.SetContentText(name)
				.SetSmallIcon(Resource.Drawable.dotnet_bot)
				.SetAutoCancel(true)
				.AddAction(0, "Taken", takenPendingIntent);

			var notificationManager = NotificationManagerCompat.From(context);
			notificationManager.Notify(reminderId, builder.Build());
		}

		static void CreateChannel(Context context)
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.O)
				return;

			var channel = new NotificationChannel(
				CHANNEL_ID,
				"Reminders",
				NotificationImportance.High);

			// Custom notification sound: Platforms/Android/Resources/raw/bottle_shake.*
			var soundUri = global::Android.Net.Uri.Parse($"android.resource://{context.PackageName}/raw/{CHANNEL_SOUND_NAME}");
			var attributes = new AudioAttributes.Builder()
				.SetUsage(AudioUsageKind.Notification)
				.SetContentType(AudioContentType.Sonification)
				.Build();
			channel.SetSound(soundUri, attributes);

			var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
			manager.CreateNotificationChannel(channel);
		}
	}
}
