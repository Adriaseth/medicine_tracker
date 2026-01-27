using medicine_tracker.Models;
using medicine_tracker.Services;

namespace medicine_tracker.Pages;

public partial class AddReminderPage : ContentPage
{
	private readonly ReminderRepository _repo;

	// Stores button -> bitmask value
	private readonly Dictionary<Button, int> _dayButtons = new();

	public AddReminderPage(ReminderRepository repo)
	{
		InitializeComponent();
		_repo = repo;
		BuildDayButtons();
		TimePicker.Time = DateTime.Now.TimeOfDay;
	}

	private void BuildDayButtons()
	{
		// Monday = bit 0 ... Sunday = bit 6
		string[] labels = { "M", "T", "W", "T", "F", "S", "S" };

		for (int i = 0; i < 7; i++)
		{
			int bit = 1 << i;

			var btn = new Button
			{
				Text = labels[i],
				WidthRequest = 46,
				HeightRequest = 46,
				CornerRadius = 23,
				BackgroundColor = Colors.LightGray,
				TextColor = Colors.Black
			};

			btn.Clicked += (s, e) =>
			{
				if (_dayButtons[btn] == 0)
				{
					_dayButtons[btn] = bit;
					btn.BackgroundColor = Colors.DodgerBlue;
					btn.TextColor = Colors.White;
				}
				else
				{
					_dayButtons[btn] = 0;
					btn.BackgroundColor = Colors.LightGray;
					btn.TextColor = Colors.Black;
				}
			};

			_dayButtons[btn] = 0;
			DaysRow.Add(btn);
		}
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{
		int mask = _dayButtons.Values.Sum();

		// No day selected = every day
		if (mask == 0)
			mask = 0b1111111;

		var time = TimePicker.Time;

		if (time != null)
		{
			var name = (NameEntry.Text ?? string.Empty).Trim();
			if (string.IsNullOrWhiteSpace(name))
			{
				await DisplayAlert("Missing name", "Please enter a reminder name.", "OK");
				NameEntry.Focus();
				return;
			}

			var reminder = new Reminder
			{
				Name = name,
				DaysMask = mask,
				Hour = time.Value.Hours,
				Minute = time.Value.Minutes
			};

			// Compute first upcoming trigger
			var next = ReminderScheduler.ComputeNextTrigger(reminder);
			reminder.NextTriggerTicks = next.ToUniversalTime().Ticks;
			reminder.IsScheduled = false;

			// Save to SQLite
			await _repo.Add(reminder);

#if ANDROID
			// Schedule native Android alarm
			if (Platforms.Android.Services.AlarmScheduler.TryScheduleReminder(reminder.Id, next, reminder.Name))
				await _repo.UpdateIsScheduled(reminder.Id, true);
#endif

			// Return to main page
			await Navigation.PopAsync();
		}
	}
}
