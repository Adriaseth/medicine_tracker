using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Util;
using Application = Android.App.Application;

//first one broken, move permission

namespace medicine_tracker.Platforms.Android.Services
{
	public static class AlarmScheduler
	{
		public const string ExtraReminderId = "reminder_id";
		public const string ExtraReminderName = "reminder_name";
		public const string ExtraAlarmKind = "alarm_kind";
		const string AlarmKindRegular = "regular";
		const string AlarmKindFollowUp = "follow_up";
		const int FollowUpRequestCodeOffset = 1000000;

		static int GetRequestCode(int reminderId, string kind)
			=> kind == AlarmKindFollowUp ? reminderId + FollowUpRequestCodeOffset : reminderId;

		public static bool TryScheduleReminder(int reminderId, DateTime time, string name)
		{
			var context = Application.Context;
			Log.Info("AlarmScheduler", $"Scheduling reminder {reminderId} '{name}' for local {time:yyyy-MM-dd HH:mm:ss} (Kind={time.Kind})");
			var intent = new Intent(context, typeof(Receivers.AlarmReceiver));
			intent.PutExtra(ExtraReminderId, reminderId);
			intent.PutExtra(ExtraReminderName, name ?? string.Empty);
			intent.PutExtra(ExtraAlarmKind, AlarmKindRegular);
			var pendingIntent = PendingIntent.GetBroadcast(
				context, GetRequestCode(reminderId, AlarmKindRegular), intent,
				PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

			long triggerTime = new DateTimeOffset(DateTime.SpecifyKind(time, DateTimeKind.Local)).ToUnixTimeMilliseconds();
			Log.Info("AlarmScheduler", $"Trigger (epoch ms) = {triggerTime}");

			var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
			if (!EnsureExactAlarmPermission(context, alarmManager))
			{
				Log.Warn("AlarmScheduler", "Exact alarm permission not granted; opened settings and did not schedule.");
				return false;
			}
			alarmManager.SetExactAndAllowWhileIdle(
				AlarmType.RtcWakeup,
				triggerTime,
				pendingIntent);
			return true;
		}

		public static void ScheduleFollowUp(int reminderId, DateTime time, string name)
		{
			var context = Application.Context;
			Log.Info("AlarmScheduler", $"Scheduling follow-up for {reminderId} '{name}' at local {time:yyyy-MM-dd HH:mm:ss}");
			var intent = new Intent(context, typeof(Receivers.AlarmReceiver));
			intent.PutExtra(ExtraReminderId, reminderId);
			intent.PutExtra(ExtraReminderName, name ?? string.Empty);
			intent.PutExtra(ExtraAlarmKind, AlarmKindFollowUp);
			var pendingIntent = PendingIntent.GetBroadcast(
				context, GetRequestCode(reminderId, AlarmKindFollowUp), intent,
				PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

			long triggerTime = new DateTimeOffset(DateTime.SpecifyKind(time, DateTimeKind.Local)).ToUnixTimeMilliseconds();
			var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
			if (!EnsureExactAlarmPermission(context, alarmManager))
				return;

			alarmManager.SetExactAndAllowWhileIdle(
				AlarmType.RtcWakeup,
				triggerTime,
				pendingIntent);
		}

		public static void ScheduleReminder(int reminderId, DateTime time, string name)
		{
			_ = TryScheduleReminder(reminderId, time, name);
		}

		public static void CancelReminder(int reminderId)
		{
			var context = Application.Context;
			var intent = new Intent(context, typeof(Receivers.AlarmReceiver));
			var pendingIntent = PendingIntent.GetBroadcast(
				context, GetRequestCode(reminderId, AlarmKindRegular), intent,
				PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);

			if (pendingIntent == null)
				return;

			var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
			alarmManager.Cancel(pendingIntent);
			pendingIntent.Cancel();
		}

		public static void CancelFollowUp(int reminderId)
		{
			var context = Application.Context;
			var intent = new Intent(context, typeof(Receivers.AlarmReceiver));
			var pendingIntent = PendingIntent.GetBroadcast(
				context, GetRequestCode(reminderId, AlarmKindFollowUp), intent,
				PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);

			if (pendingIntent == null)
				return;

			var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
			alarmManager.Cancel(pendingIntent);
			pendingIntent.Cancel();
		}

		public static void CancelAllForReminder(int reminderId)
		{
			CancelReminder(reminderId);
			CancelFollowUp(reminderId);
		}

		public static bool EnsureExactAlarmPermission(Context context, AlarmManager? alarmManager = null)
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
			{
				alarmManager ??= (AlarmManager)context.GetSystemService(Context.AlarmService);
				if (!alarmManager.CanScheduleExactAlarms())
				{
					// Opens system settings where user can allow exact alarms
					var intent = new Intent(Settings.ActionRequestScheduleExactAlarm);
					intent.SetFlags(ActivityFlags.NewTask);
					context.StartActivity(intent);
					return false;
				}
			}

			return true;
		}
	}
}
