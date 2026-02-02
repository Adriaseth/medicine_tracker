using System;
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
			var alarmKind = intent.GetStringExtra(AlarmScheduler.ExtraAlarmKind) ?? "";
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
				// Swiping the notification away does NOT mean taken.
				// The next regular occurrence is only scheduled when the user marks it as taken.
				if (!reminder.IsTaken)
				{
					await repo.IncrementFollowUp(reminderId);
					// Avoid scheduling multiple follow-ups for a single follow-up trigger.
					if (alarmKind != "follow_up")
					{
						var followUp = DateTime.Now.AddMinutes(10);
						AlarmScheduler.ScheduleFollowUp(reminderId, followUp, reminder.Name);
					}
					return;
				}

				// If taken is already set, do nothing here.
				// Scheduling the next regular occurrence is handled by the Taken action.
			}
			catch
			{
				// Swallow exceptions to avoid crashing the broadcast receiver.
			}
		}
	}
}
