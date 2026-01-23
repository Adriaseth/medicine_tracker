namespace medicine_tracker
{
	public partial class App : Application
	{
		public App(MainPage page)
		{
			InitializeComponent();
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