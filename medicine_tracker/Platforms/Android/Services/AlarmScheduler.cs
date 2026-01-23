using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Application = Android.App.Application;

namespace medicine_tracker.Platforms.Android.Services
{
	public static class AlarmScheduler
	{
		const string ExtraReminderName = "reminder_name";

		public static void ScheduleReminder(DateTime time, string name)
		{
			var context = Application.Context;
			var intent = new Intent(context, typeof(Receivers.AlarmReceiver));
			intent.PutExtra(ExtraReminderName, name ?? string.Empty);
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
