using SQLite;

namespace medicine_tracker.Services.Database.Migrations;

public sealed class Migration002_AddReminderIsScheduled : IMigration
{
	public int Version => 2;

	public Task Apply(SQLiteAsyncConnection db)
	{
		// Existing rows get the DEFAULT (0/false).
		return db.ExecuteAsync(
			$"ALTER TABLE {DbSchema.ReminderTableName} ADD COLUMN IsScheduled INTEGER NOT NULL DEFAULT 0;");
	}
}
