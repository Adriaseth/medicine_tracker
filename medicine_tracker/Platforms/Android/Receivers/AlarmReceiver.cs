using Android.Content;
using Android.Util;
using Microsoft.Maui;
using medicine_tracker.Platforms.Android.Services;

using medicine_tracker.Services;

namespace medicine_tracker.Platforms.Android.Receivers
{
	[BroadcastReceiver(Enabled = true, Exported = false)]
	public class AlarmReceiver : BroadcastReceiver
	{
		public override async void OnReceive(Context context, Intent intent)
		{
			var reminderId = intent.GetIntExtra(AlarmScheduler.ExtraReminderId, 0);
			var name = intent.GetStringExtra(AlarmScheduler.ExtraReminderName);
			Log.Info("AlarmReceiver", $"Received alarm. reminderId={reminderId}, name='{name}'");

			NotificationHelper.ShowNotification(context, name);

			if (reminderId <= 0)
				return;

			try
			{
				var repo = MauiApplication.Current.Services.GetService(typeof(ReminderRepository)) as ReminderRepository;
				if (repo == null)
					return;

				var reminder = await repo.GetById(reminderId);
				if (reminder == null)
					return;

				var next = medicine_tracker.Services.ReminderScheduler.ComputeNextTrigger(reminder);
				await repo.UpdateNextTrigger(reminderId, next);

				AlarmScheduler.ScheduleReminder(reminderId, next, reminder.Name);
			}
			catch
			{
				// Swallow exceptions to avoid crashing the broadcast receiver.
			}
		}
	}
}
