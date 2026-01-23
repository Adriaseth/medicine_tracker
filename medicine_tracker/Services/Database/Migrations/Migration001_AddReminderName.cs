using SQLite;

namespace medicine_tracker.Services.Database.Migrations;

public sealed class Migration001_AddReminderName : IMigration
{
	public int Version => 1;

	public Task Apply(SQLiteAsyncConnection db)
	{
		// Existing rows get the DEFAULT.
		return db.ExecuteAsync(
			$"ALTER TABLE {DbSchema.ReminderTableName} ADD COLUMN Name TEXT NOT NULL DEFAULT '';");
	}
}
