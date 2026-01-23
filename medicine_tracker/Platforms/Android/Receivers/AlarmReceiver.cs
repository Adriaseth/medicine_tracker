using Android.Content;

namespace medicine_tracker.Platforms.Android.Receivers
{
	[BroadcastReceiver(Enabled = true, Exported = false)]
	public class AlarmReceiver : BroadcastReceiver
	{
		const string ExtraReminderName = "reminder_name";

		public override void OnReceive(Context context, Intent intent)
		{
			var name = intent.GetStringExtra(ExtraReminderName);
			NotificationHelper.ShowNotification(context, name);
		}
	}
}
