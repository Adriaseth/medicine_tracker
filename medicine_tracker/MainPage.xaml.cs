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
				RemindersList.ItemsSource = await _repo.GetAll();
			}
			catch
			{
				RemindersList.ItemsSource = Array.Empty<Models.Reminder>();
			}
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
			Application.Current!.UserAppTheme = current == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
		}
	}
}
