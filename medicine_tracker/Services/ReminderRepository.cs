using medicine_tracker.Models;
using medicine_tracker.Services.Database;
using medicine_tracker.Services.Database.Migrations;
using SQLite;

namespace medicine_tracker.Services
{
	public class ReminderRepository
	{
		SQLiteAsyncConnection _db;
		static readonly IMigration[] Migrations =
		[
			new Migration001_AddReminderName(),
			new Migration002_AddReminderIsScheduled(),
			new Migration003_AddReminderSmartFields(),
		];

		async Task Init()
		{
			if (_db != null) return;

			var path = Path.Combine(
				FileSystem.AppDataDirectory,
				DbSchema.DatabaseFileName);

			_db = new SQLiteAsyncConnection(path);
			await ApplyMigrations();
			// Safety net: ensure table exists even if the DB is new/partial.
			await _db.CreateTableAsync<Reminder>();
		}

		async Task ApplyMigrations()
		{
			var migrator = new SqliteDbMigrator(_db, Migrations);
			await migrator.MigrateIfNeeded(async () =>
			{
				// For a brand-new DB, this creates the table with the *latest* schema
				// (including the Name column), and no ALTER TABLE migrations are needed.
				await _db.CreateTableAsync<Reminder>();
			});
		}

		public async Task<List<Reminder>> GetAll()
		{
			await Init();
			return await _db.Table<Reminder>().ToListAsync();
		}

		public async Task<Reminder?> GetById(int id)
		{
			await Init();
			return await _db.Table<Reminder>().Where(r => r.Id == id).FirstOrDefaultAsync();
		}

		public async Task Add(Reminder r)
		{
			await Init();
			await _db.InsertAsync(r);
		}

		public async Task UpdateIsScheduled(int id, bool isScheduled)
		{
			await Init();
			await _db.ExecuteAsync(
				"UPDATE Reminder SET IsScheduled = ? WHERE Id = ?",
				isScheduled ? 1 : 0,
				id);
		}

		public async Task Update(Reminder r)
		{
			await Init();
			await _db.UpdateAsync(r);
		}

		public async Task UpdateNextTrigger(int id, DateTime nextTriggerLocal)
		{
			await Init();
			var nextUtcTicks = nextTriggerLocal.ToUniversalTime().Ticks; // Ensure UTC ticks are stored
			await _db.ExecuteAsync(
				"UPDATE Reminder SET NextTriggerTicks = ? WHERE Id = ?",
				nextUtcTicks,
				id);
		}

		public async Task ResetSmartState(int id)
		{
			await Init();
			await _db.ExecuteAsync(
				"UPDATE Reminder SET IsTaken = 0, FollowUpCount = 0 WHERE Id = ?",
				id);
		}

		public async Task MarkTaken(int id)
		{
			await Init();
			await _db.ExecuteAsync(
				"UPDATE Reminder SET IsTaken = 1, FollowUpCount = 0 WHERE Id = ?",
				id);
		}

		public async Task IncrementFollowUp(int id)
		{
			await Init();
			await _db.ExecuteAsync(
				"UPDATE Reminder SET FollowUpCount = FollowUpCount + 1 WHERE Id = ?",
				id);
		}
	}
}
