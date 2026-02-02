using medicine_tracker.Pages;
using medicine_tracker.Services;
using System.Linq;

#if ANDROID
using Android.OS;
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace medicine_tracker
{
	public partial class MainPage : ContentPage
	{
		readonly ReminderRepository _repo;
		bool _resumeRefreshAttached;
		bool _timerRunning;

		public MainPage(ReminderRepository repo)
		{
			InitializeComponent();
			_repo = repo;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			AttachResumeRefresh();
			StartRefreshTimer();

#if ANDROID
			RequestNotificationPermissionAndroid();
			try
			{
				await EnsureAlarmsScheduledAndroid();
			}
			catch
			{
				// If DB isn't initialized yet, avoid crashing on startup.
			}
#endif

			await RefreshRemindersAsync();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_timerRunning = false;
		}

		async Task RefreshRemindersAsync()
		{
			try
			{
				var reminders = await _repo.GetAll();
				var today = DateTime.Now.Date;
				foreach (var r in reminders)
				{
					// Keep it green after being taken until the day it becomes due again.
					// Once we reach the next trigger date, reset IsTaken so the reminder shows as pending.
					var nextLocal = r.NextTrigger;
					if (r.IsTaken && today >= nextLocal.Date)
					{
						r.IsTaken = false;
						r.FollowUpCount = 0;
						await _repo.Update(r);
					}
				}
				RemindersList.ItemsSource = reminders;
			}
			catch
			{
				RemindersList.ItemsSource = Array.Empty<Models.Reminder>();
			}
		}

		async void OnReminderTapped(object sender, TappedEventArgs e)
		{
			if (e.Parameter is not Models.Reminder reminder)
				return;

			if (reminder.IsTaken)
				return;

			await _repo.MarkTaken(reminder.Id);

			// Schedule the next regular reminder time.
			var refreshed = await _repo.GetById(reminder.Id);
			if (refreshed != null)
			{
				var next = Services.ReminderScheduler.ComputeNextTrigger(refreshed);
				await _repo.UpdateNextTrigger(refreshed.Id, next);

#if ANDROID
				Platforms.Android.Services.AlarmScheduler.CancelAllForReminder(refreshed.Id);
				Platforms.Android.Services.AlarmScheduler.ScheduleReminder(refreshed.Id, next, refreshed.Name);
#endif
			}

			await RefreshRemindersAsync();
		}

		async void OnDeleteInvoked(object sender, EventArgs e)
		{
			if (sender is not SwipeItem swipeItem)
				return;
			if (swipeItem.CommandParameter is not Models.Reminder reminder)
				return;

			var ok = await DisplayAlert(
				"Delete reminder",
				$"Delete '{reminder.Name}'?",
				"Delete",
				"Cancel");
			if (!ok)
				return;

#if ANDROID
			Platforms.Android.Services.AlarmScheduler.CancelReminder(reminder.Id);
#endif

			await _repo.Delete(reminder.Id);
			await RefreshRemindersAsync();
		}

		void AttachResumeRefresh()
		{
			if (_resumeRefreshAttached)
				return;
			_resumeRefreshAttached = true;

			// MAUI doesn't expose Application.Resumed. Use the Window lifecycle event instead.
			var window = Application.Current?.Windows.FirstOrDefault();
			if (window == null)
				return;

			window.Resumed += async (_, __) =>
			{
				await RefreshRemindersAsync();
			};
		}

		void StartRefreshTimer()
		{
			if (_timerRunning)
				return;
			_timerRunning = true;

			Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
			{
				if (!_timerRunning)
					return false;

				_ = RefreshRemindersAsync();
				return true;
			});
		}

#if ANDROID
		bool _alarmsEnsured;

		async Task EnsureAlarmsScheduledAndroid()
		{
			if (_alarmsEnsured)
				return;
			_alarmsEnsured = true;

			var reminders = await _repo.GetAll();
			foreach (var reminder in reminders)
			{
				// If the reminder is currently awaiting confirmation (not taken) and already due,
				// keep the nag cycle going instead of overwriting with the next regular occurrence.
				var nextDue = reminder.NextTrigger;
				if (!reminder.IsTaken && DateTime.Now >= nextDue)
				{
					var followUp = DateTime.Now.AddMinutes(10);
					Platforms.Android.Services.AlarmScheduler.ScheduleFollowUp(reminder.Id, followUp, reminder.Name);
					continue;
				}

				var next = Services.ReminderScheduler.ComputeNextTrigger(reminder);
				await _repo.UpdateNextTrigger(reminder.Id, next);
				if (Platforms.Android.Services.AlarmScheduler.TryScheduleReminder(reminder.Id, next, reminder.Name))
					await _repo.UpdateIsScheduled(reminder.Id, true);
			}
		}
#endif

#if ANDROID
		void RequestNotificationPermissionAndroid()
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
				return;

			var context = Android.App.Application.Context;

			if (ContextCompat.CheckSelfPermission(context, Manifest.Permission.PostNotifications)
				!= Permission.Granted)
			{
				ActivityCompat.RequestPermissions(
					Platform.CurrentActivity!,
					new[] { Manifest.Permission.PostNotifications },
					101);
			}
		}
#endif

		async void OnAddClicked(object sender, EventArgs e)
		{
		#if ANDROID
			var context = global::Android.App.Application.Context;
			if (!Platforms.Android.Services.AlarmScheduler.EnsureExactAlarmPermission(context))
			{
				await DisplayAlert(
					"Allow exact alarms",
					"To schedule reminders precisely, enable 'Alarms & reminders' for this app. After enabling it, tap + again.",
					"OK");
				return;
			}
		#endif
			await Navigation.PushAsync(new AddReminderPage(_repo));
		}

		void OnToggleThemeClicked(object sender, EventArgs e)
		{
			var current = Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
			var next = current == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
			Application.Current!.UserAppTheme = next;
			App.SaveTheme(next);
		}
	}
}
