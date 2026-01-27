using SQLite;

namespace medicine_tracker.Services.Database.Migrations;

public sealed class Migration003_AddReminderSmartFields : IMigration
{
	public int Version => 3;

	public async Task Apply(SQLiteAsyncConnection db)
	{
		// Existing rows get defaults.
		await db.ExecuteAsync(
			$"ALTER TABLE {DbSchema.ReminderTableName} ADD COLUMN IsTaken INTEGER NOT NULL DEFAULT 0;");
		await db.ExecuteAsync(
			$"ALTER TABLE {DbSchema.ReminderTableName} ADD COLUMN FollowUpCount INTEGER NOT NULL DEFAULT 0;");
	}
}
