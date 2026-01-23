using Android.Content;

namespace medicine_tracker.Platforms.Android.Receivers
{
	[BroadcastReceiver(Enabled = true, Exported = false)]
	public class AlarmReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			NotificationHelper.ShowNotification(context);
		}
	}
}
