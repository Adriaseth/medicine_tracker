using SQLite;

namespace medicine_tracker.Services.Database;

public sealed class SqliteDbMigrator
{
	const string UserVersionPragma = "PRAGMA user_version;";

	readonly SQLiteAsyncConnection _db;
	readonly IReadOnlyList<IMigration> _migrations;

	public SqliteDbMigrator(SQLiteAsyncConnection db, IEnumerable<IMigration> migrations)
	{
		_db = db;
		_migrations = migrations.OrderBy(m => m.Version).ToList();
	}

	public int LatestVersion => _migrations.Count == 0 ? 0 : _migrations[^1].Version;

	public async Task MigrateIfNeeded(Func<Task> ensureSchemaCreated)
	{
		if (ensureSchemaCreated == null)
			throw new ArgumentNullException(nameof(ensureSchemaCreated));

		var currentVersion = await _db.ExecuteScalarAsync<int>(UserVersionPragma);
		if (currentVersion >= LatestVersion)
			return;

		await ensureSchemaCreated();

		foreach (var migration in _migrations)
		{
			if (currentVersion >= migration.Version)
				continue;

			await migration.Apply(_db);
			currentVersion = migration.Version;
			await SetUserVersion(currentVersion);
		}
	}

	async Task SetUserVersion(int version)
	{
		await _db.ExecuteAsync($"PRAGMA user_version = {version};");
	}
}
