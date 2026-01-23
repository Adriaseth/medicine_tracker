using SQLite;

namespace medicine_tracker.Models
{
	public class Reminder
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		// Bitmask for days: 0 = none selected → treat as all
		public int DaysMask { get; set; }

		// Time of day
		public int Hour { get; set; }
		public int Minute { get; set; }

		// Next scheduled trigger (UTC ticks)
		public long NextTriggerTicks { get; set; }
	}
}
