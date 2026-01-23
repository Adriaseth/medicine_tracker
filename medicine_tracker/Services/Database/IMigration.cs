using SQLite;

namespace medicine_tracker.Services.Database;

public interface IMigration
{
	int Version { get; }
	Task Apply(SQLiteAsyncConnection db);
}
