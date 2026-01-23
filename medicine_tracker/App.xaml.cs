namespace medicine_tracker
{
	public partial class App : Application
	{
		public App(MainPage page)
		{
			InitializeComponent();
			MainPage = new NavigationPage(page);
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			return new Window(new AppShell());
		}
	}
}