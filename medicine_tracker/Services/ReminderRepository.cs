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
		];

		async Task Init()
		{
			if (_db != null) return;

			var path = Path.Combine(
				FileSystem.AppDataDirectory,
				DbSchema.DatabaseFileName);

			_db = new SQLiteAsyncConnection(path);
			await ApplyMigrations();
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

		public async Task Add(Reminder r)
		{
			await Init();
			await _db.InsertAsync(r);
		}
	}
}
