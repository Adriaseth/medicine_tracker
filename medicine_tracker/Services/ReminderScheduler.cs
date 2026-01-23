using medicine_tracker.Models;

namespace medicine_tracker.Services
{
	public static class ReminderScheduler
	{
		public static DateTime ComputeNextTrigger(Reminder r)
		{
			var now = DateTime.Now;

			// No days selected = every day
			int mask = r.DaysMask == 0 ? 127 : r.DaysMask;

			for (int i = 0; i < 7; i++)
			{
				var day = now.Date.AddDays(i);
				int dayBit = 1 << (((int)day.DayOfWeek + 6) % 7);
				// convert Sunday=0 → bit 64

				if ((mask & dayBit) == 0)
					continue;

				var candidate = new DateTime(
					day.Year, day.Month, day.Day,
					r.Hour, r.Minute, 0);

				if (candidate > now)
					return candidate;
			}

			// fallback tomorrow
			return now.AddDays(1);
		}
	}
}
