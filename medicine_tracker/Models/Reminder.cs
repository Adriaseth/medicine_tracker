using SQLite;

namespace medicine_tracker.Models
{
	public class Reminder
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

		// Bitmask for days: 0 = none selected → treat as all
		public int DaysMask { get; set; }

		// Time of day
		public int Hour { get; set; }
		public int Minute { get; set; }

		// Next scheduled trigger (UTC ticks)
		public long NextTriggerTicks { get; set; }

		// Whether an alarm has been scheduled for NextTriggerTicks
		public bool IsScheduled { get; set; }

		// Whether the currently due dose has been acknowledged via notification action.
		// When false, the app will send a reminder notification every 10 minutes.
		public bool IsTaken { get; set; }

		// Number of 10-minute follow-up reminders sent for the current due dose.
		public int FollowUpCount { get; set; }

		[Ignore]
		public DateTime NextTrigger =>
			DateTime.SpecifyKind(new DateTime(NextTriggerTicks), DateTimeKind.Utc).ToLocalTime();
	}
}
