namespace medicine_tracker
{
	public partial class App : Application
	{
		const string ThemePreferenceKey = "theme_preference";

		public App(MainPage page)
		{
			InitializeComponent();
			ApplySavedTheme();
		}

		static void ApplySavedTheme()
		{
			var value = Preferences.Get(ThemePreferenceKey, string.Empty);
			if (value == nameof(AppTheme.Dark))
				Current!.UserAppTheme = AppTheme.Dark;
			else if (value == nameof(AppTheme.Light))
				Current!.UserAppTheme = AppTheme.Light;
		}

		public static void SaveTheme(AppTheme theme)
		{
			Preferences.Set(ThemePreferenceKey, theme.ToString());
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			// Trigger DB initialization/migrations early.
			// Keep it Android-only to avoid relying on platform-specific entrypoints during multi-target builds.
#if ANDROID
			var services = Current?.Handler?.MauiContext?.Services;
			if (services != null)
				_ = services.GetService<Services.ReminderRepository>()?.GetAll();
#endif
			return new Window(new AppShell());
		}
	}
}