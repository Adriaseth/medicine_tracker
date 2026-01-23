using medicine_tracker.Models;
using SQLite;

namespace medicine_tracker.Services
{
	public class ReminderRepository
	{
		SQLiteAsyncConnection _db;

		async Task Init()
		{
			if (_db != null) return;

			var path = Path.Combine(
				FileSystem.AppDataDirectory,
				"reminders.db");

			_db = new SQLiteAsyncConnection(path);
			await _db.CreateTableAsync<Reminder>();
		}

		public async Task<List<Reminder>> GetAll()
		{
			await Init();
			return await _db.Table<Reminder>().ToListAsync();
		}

		public async Task Add(Reminder r)
		{
			await Init();
			await _db.InsertAsync(r);
		}
	}
}
