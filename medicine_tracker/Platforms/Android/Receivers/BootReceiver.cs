using Android.Content;
using Android.App;
using medicine_tracker.Services;
using medicine_tracker.Platforms.Android.Services;
using Microsoft.Maui;

namespace medicine_tracker.Platforms.Android.Receivers;

[BroadcastReceiver(Enabled = true, Exported = false, DirectBootAware = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted })]
public class BootReceiver : BroadcastReceiver
{
	public override async void OnReceive(Context context, Intent intent)
	{
		try
		{
			var repo = MauiApplication.Current.Services.GetService(typeof(ReminderRepository)) as ReminderRepository;
			if (repo == null)
				return;

			var reminders = await repo.GetAll();
			foreach (var r in reminders)
			{
				var next = medicine_tracker.Services.ReminderScheduler.ComputeNextTrigger(r);
				await repo.UpdateNextTrigger(r.Id, next);
				AlarmScheduler.ScheduleReminder(r.Id, next, r.Name);
			}
		}
		catch
		{
			// Best-effort reschedule
		}
	}
}
