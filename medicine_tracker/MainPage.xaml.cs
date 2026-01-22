
using Android.OS;
#if ANDROID
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace medicine_tracker
{
    public partial class MainPage : ContentPage
    {
        private int _reminderId = 1000;
        private bool _reminderActive = false;

        public MainPage()
        {
            InitializeComponent();
		}

        protected override void OnAppearing()
        {
	        base.OnAppearing();

#if ANDROID
	        RequestNotificationPermissionAndroid();
#endif
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

		private void OnScheduleClicked(object sender, EventArgs e)
		{
#if ANDROID
			Platforms.Android.Services.AlarmScheduler
				.ScheduleReminder(DateTime.Now.AddMinutes(1));
#endif
		}
	}
}
