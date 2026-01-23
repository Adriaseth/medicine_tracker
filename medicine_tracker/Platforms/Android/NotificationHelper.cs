using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace medicine_tracker.Platforms.Android
{
	public static class NotificationHelper
	{
		const string CHANNEL_ID = "reminder_channel";

		public static void ShowNotification(Context context, string? reminderName = null)
		{
			CreateChannel(context);
			var name = string.IsNullOrWhiteSpace(reminderName) ? "Reminder" : reminderName;

			var builder = new NotificationCompat.Builder(context, CHANNEL_ID)
				.SetContentTitle("Reminder")
				.SetContentText(name)
				.SetSmallIcon(Resource.Drawable.dotnet_bot)
				.SetAutoCancel(true);

			var notificationManager = NotificationManagerCompat.From(context);
			notificationManager.Notify(1001, builder.Build());
		}

		static void CreateChannel(Context context)
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.O)
				return;

			var channel = new NotificationChannel(
				CHANNEL_ID,
				"Reminders",
				NotificationImportance.High);

			var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
			manager.CreateNotificationChannel(channel);
		}
	}
}
