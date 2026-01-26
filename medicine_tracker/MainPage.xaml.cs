using medicine_tracker.Pages;
using medicine_tracker.Services;

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

		public MainPage(ReminderRepository repo)
		{
			InitializeComponent();
			_repo = repo;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

#if ANDROID
			RequestNotificationPermissionAndroid();
#endif

			RemindersList.ItemsSource = await _repo.GetAll();
		}

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
			await Navigation.PushAsync(new AddReminderPage(_repo));
		}

		void OnToggleThemeClicked(object sender, EventArgs e)
		{
			var current = Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
			Application.Current!.UserAppTheme = current == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
		}
	}
}
