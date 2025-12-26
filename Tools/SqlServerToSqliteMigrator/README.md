# SqlServerToSqliteMigrator

Short notes for running the migrator tool in this repo.

## Sensitive rows & delete workflow 🔒
- Use `--include-sensitive` to include rows marked as sensitive during migration. When included, the migrator records each migrated sensitive row in the target DB table `MigratedSensitiveRows`.
- Use `--generate-delete-script` to create `delete-sensitive.sql` after migration. This file contains DELETE statements for the migrated sensitive rows so they can be removed later if required.
- The migrator writes `delete-sensitive.sql` to the working directory when `--generate-delete-script` is specified.

## Recommended deterministic run ✅
- For a deterministic import (no duplicates and no key conflicts), run the migrator against a clean target DB file (remove or backup the existing SQLite DB before running).
- The tool attempts to avoid duplicate inserts and will use `INSERT OR IGNORE` for raw-SQL fallback inserts; however, pre-clearing the target DB ensures a fully repeatable import.

## Example

dotnet run --project Tools/SqlServerToSqliteMigrator -- --source-connection "<connection>" --include-sensitive --generate-delete-script --tables remaining --batch-size 1000 --verify

