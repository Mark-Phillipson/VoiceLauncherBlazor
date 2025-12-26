# DataAccessLibrary - Deprecation Notes

- The Dapper-based `TodoData` implementation has been deprecated.
- Use `TodoDataEf` (Entity Framework Core) as the canonical implementation for Todos.
- `TodoData` now throws NotSupportedException and is marked with `[Obsolete]` to prevent accidental usage.

If you need to fully remove the file from the repository, remove it and any references in other projects; currently DI is bound to `TodoDataEf` so removal is safe.
