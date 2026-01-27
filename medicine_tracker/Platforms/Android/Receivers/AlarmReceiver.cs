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

			NotificationHelper.ShowNotification(context, reminderId, name);

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

				// If not taken yet, schedule a follow-up in 10 minutes.
				if (!reminder.IsTaken)
				{
					await repo.IncrementFollowUp(reminderId);
					var followUp = DateTime.Now.AddMinutes(10);
					AlarmScheduler.ScheduleReminder(reminderId, followUp, reminder.Name);
					return;
				}

				// Taken: schedule the next regular dose.
				await repo.ResetSmartState(reminderId);
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
