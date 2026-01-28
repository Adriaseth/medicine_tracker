using Android.Content;
using Android.Util;
using AndroidX.Core.App;
using Microsoft.Maui;
using medicine_tracker.Services;
using medicine_tracker.Platforms.Android.Services;

namespace medicine_tracker.Platforms.Android.Receivers;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class NotificationActionReceiver : BroadcastReceiver
{
	public const string ActionTaken = "medicine_tracker.action.TAKEN";

	public override async void OnReceive(Context context, Intent intent)
	{
		if (intent?.Action != ActionTaken)
			return;

		var reminderId = intent.GetIntExtra(AlarmScheduler.ExtraReminderId, 0);
		Log.Info("NotificationAction", $"TAKEN clicked. reminderId={reminderId}");
		if (reminderId <= 0)
			return;

		// Dismiss the current notification.
		try
		{
			NotificationManagerCompat.From(context).Cancel(reminderId);
		}
		catch
		{
			// best effort
		}

		try
		{
			var repo = MauiApplication.Current.Services.GetService(typeof(ReminderRepository)) as ReminderRepository;
			if (repo == null)
				return;

			await repo.MarkTaken(reminderId);

			var reminder = await repo.GetById(reminderId);
			if (reminder == null)
				return;

			// Compute and schedule the next regular dose time.
			var next = ReminderScheduler.ComputeNextTrigger(reminder);
			await repo.UpdateNextTrigger(reminderId, next);
			await repo.UpdateIsScheduled(reminderId, true);
			AlarmScheduler.ScheduleReminder(reminderId, next, reminder.Name);
		}
		catch
		{
			// best effort
		}
	}
}
