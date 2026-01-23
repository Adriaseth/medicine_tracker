using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Application = Android.App.Application;

namespace medicine_tracker.Platforms.Android.Services
{
	public static class AlarmScheduler
	{
		public static void ScheduleReminder(DateTime time)
		{
			var context = Application.Context;
			var intent = new Intent(context, typeof(Receivers.AlarmReceiver));
			var pendingIntent = PendingIntent.GetBroadcast(
				context, 0, intent,
				PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

			long triggerTime = new DateTimeOffset(time).ToUnixTimeMilliseconds();

			var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
			EnsureExactAlarmPermission(context);
			alarmManager.SetExactAndAllowWhileIdle(
				AlarmType.RtcWakeup,
				triggerTime,
				pendingIntent);
		}

		public static void EnsureExactAlarmPermission(Context context)
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
			{
				var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
				if (!alarmManager.CanScheduleExactAlarms())
				{
					// Opens system settings where user can allow exact alarms
					var intent = new Intent(Settings.ActionRequestScheduleExactAlarm);
					intent.SetFlags(ActivityFlags.NewTask);
					context.StartActivity(intent);
				}
			}
		}
	}
}
