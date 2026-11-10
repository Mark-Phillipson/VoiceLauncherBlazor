using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security;

namespace WinFormsApp;

/// <summary>
/// Maintains a persistent folder-name index and searches it without walking the drive for each query.
/// </summary>
internal sealed class FolderWalkerService
{
    public static FolderWalkerService Shared { get; } = new();

    private readonly string _connectionString;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _driveLocks =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly object _schemaLock = new();
    private bool _schemaInitialized;
    private bool _supportsTrigramSearch;

    private FolderWalkerService()
    {
        var indexDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoiceLauncherBlazor");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(indexDirectory, "folder-index.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public async Task<FolderIndexSummary> EnsureIndexAsync(
        string rootPath,
        IProgress<FolderWalkerIndexProgress>? progress = null,
        CancellationToken cancellationToken = default,
        bool forceRefresh = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        var normalizedRoot = Path.GetPathRoot(Path.GetFullPath(rootPath)) ?? rootPath;
        var driveLock = _driveLocks.GetOrAdd(normalizedRoot, _ => new SemaphoreSlim(1, 1));

        await driveLock.WaitAsync(cancellationToken);
        try
        {
            return await Task.Run(
                () => BuildOrLoadIndex(normalizedRoot, progress, cancellationToken, forceRefresh),
                cancellationToken);
        }
        finally
        {
            driveLock.Release();
        }
    }

    public async Task<FolderSearchResult> SearchAsync(
        string rootPath,
        string searchText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);

        var normalizedSearchText = searchText.Trim();
        if (normalizedSearchText.Length == 0)
            throw new ArgumentException("Enter a folder name to search for.", nameof(searchText));

        var summary = await EnsureIndexAsync(rootPath, cancellationToken: cancellationToken);
        return await Task.Run(
            () => SearchIndex(summary.RootPath, normalizedSearchText, summary.SkippedFolderCount, cancellationToken),
            cancellationToken);
    }

    private FolderIndexSummary BuildOrLoadIndex(
        string rootPath,
        IProgress<FolderWalkerIndexProgress>? progress,
        CancellationToken cancellationToken,
        bool forceRefresh)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureDatabase();

        var drive = new DriveInfo(rootPath);
        if (!drive.IsReady)
            throw new IOException($"Drive {rootPath} is not ready.");

        var fingerprint = new DriveFingerprint(
            drive.VolumeLabel,
            drive.DriveFormat,
            drive.DriveType,
            drive.TotalSize);

        if (!forceRefresh && TryReadCurrentIndex(rootPath, fingerprint, out var cachedSummary))
            return cachedSummary!;

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        using (var deleteEntries = connection.CreateCommand())
        {
            deleteEntries.Transaction = transaction;
            deleteEntries.CommandText = "DELETE FROM FolderEntries WHERE DriveRoot = $root;";
            deleteEntries.Parameters.AddWithValue("$root", rootPath);
            deleteEntries.ExecuteNonQuery();
        }

        if (_supportsTrigramSearch)
        {
            using var deleteSearch = connection.CreateCommand();
            deleteSearch.Transaction = transaction;
            deleteSearch.CommandText = "DELETE FROM FolderNameSearch WHERE DriveRoot = $root;";
            deleteSearch.Parameters.AddWithValue("$root", rootPath);
            deleteSearch.ExecuteNonQuery();
        }

        using var insertEntry = connection.CreateCommand();
        insertEntry.Transaction = transaction;
        insertEntry.CommandText = """
            INSERT INTO FolderEntries (DriveRoot, FolderPath, FolderName)
            VALUES ($root, $path, $name);
            """;
        insertEntry.Parameters.Add("$root", SqliteType.Text);
        insertEntry.Parameters.Add("$path", SqliteType.Text);
        insertEntry.Parameters.Add("$name", SqliteType.Text);
        insertEntry.Prepare();

        SqliteCommand? insertSearch = null;
        if (_supportsTrigramSearch)
        {
            insertSearch = connection.CreateCommand();
            insertSearch.Transaction = transaction;
            insertSearch.CommandText = """
                INSERT INTO FolderNameSearch (FolderName, DriveRoot, FolderPath)
                VALUES ($name, $root, $path);
                """;
            insertSearch.Parameters.Add("$name", SqliteType.Text);
            insertSearch.Parameters.Add("$root", SqliteType.Text);
            insertSearch.Parameters.Add("$path", SqliteType.Text);
            insertSearch.Prepare();
        }

        var pendingFolders = new Stack<string>();
        pendingFolders.Push(rootPath);
        var folderCount = 0;
        var skippedFolderCount = 0;
        var progressTimer = Stopwatch.StartNew();

        try
        {
            while (pendingFolders.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var currentPath = pendingFolders.Pop();

                try
                {
                    foreach (var childPath in Directory.EnumerateDirectories(currentPath))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            var attributes = File.GetAttributes(childPath);
                            if ((attributes & FileAttributes.ReparsePoint) != 0)
                                continue;

                            var folderName = Path.GetFileName(childPath.TrimEnd(
                                Path.DirectorySeparatorChar,
                                Path.AltDirectorySeparatorChar)) ?? childPath;

                            insertEntry.Parameters["$root"].Value = rootPath;
                            insertEntry.Parameters["$path"].Value = childPath;
                            insertEntry.Parameters["$name"].Value = folderName;
                            insertEntry.ExecuteNonQuery();

                            if (insertSearch is not null)
                            {
                                insertSearch.Parameters["$name"].Value = folderName;
                                insertSearch.Parameters["$root"].Value = rootPath;
                                insertSearch.Parameters["$path"].Value = childPath;
                                insertSearch.ExecuteNonQuery();
                            }

                            folderCount++;
                            pendingFolders.Push(childPath);

                            if (folderCount % 2000 == 0 || progressTimer.ElapsedMilliseconds >= 750)
                            {
                                progress?.Report(new FolderWalkerIndexProgress(
                                    rootPath,
                                    folderCount,
                                    skippedFolderCount));
                                progressTimer.Restart();
                            }
                        }
                        catch (Exception exception) when (IsFileSystemAccessException(exception))
                        {
                            skippedFolderCount++;
                        }
                    }
                }
                catch (Exception exception) when (IsFileSystemAccessException(exception))
                {
                    skippedFolderCount++;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            var indexedAtUtc = DateTime.UtcNow;
            using (var saveMetadata = connection.CreateCommand())
            {
                saveMetadata.Transaction = transaction;
                saveMetadata.CommandText = """
                    INSERT INTO FolderDriveIndexes
                        (DriveRoot, VolumeLabel, DriveFormat, DriveType, TotalSize, FolderCount, SkippedFolderCount, IndexedAtUtc)
                    VALUES
                        ($root, $label, $format, $type, $size, $count, $skipped, $indexedAt)
                    ON CONFLICT(DriveRoot) DO UPDATE SET
                        VolumeLabel = excluded.VolumeLabel,
                        DriveFormat = excluded.DriveFormat,
                        DriveType = excluded.DriveType,
                        TotalSize = excluded.TotalSize,
                        FolderCount = excluded.FolderCount,
                        SkippedFolderCount = excluded.SkippedFolderCount,
                        IndexedAtUtc = excluded.IndexedAtUtc;
                    """;
                saveMetadata.Parameters.AddWithValue("$root", rootPath);
                saveMetadata.Parameters.AddWithValue("$label", fingerprint.VolumeLabel);
                saveMetadata.Parameters.AddWithValue("$format", fingerprint.DriveFormat);
                saveMetadata.Parameters.AddWithValue("$type", (int)fingerprint.DriveType);
                saveMetadata.Parameters.AddWithValue("$size", fingerprint.TotalSize);
                saveMetadata.Parameters.AddWithValue("$count", folderCount);
                saveMetadata.Parameters.AddWithValue("$skipped", skippedFolderCount);
                saveMetadata.Parameters.AddWithValue("$indexedAt", indexedAtUtc.ToString("O"));
                saveMetadata.ExecuteNonQuery();
            }

            transaction.Commit();
            progress?.Report(new FolderWalkerIndexProgress(rootPath, folderCount, skippedFolderCount));
            return new FolderIndexSummary(rootPath, folderCount, skippedFolderCount, indexedAtUtc);
        }
        finally
        {
            insertSearch?.Dispose();
        }
    }

    private FolderSearchResult SearchIndex(
        string rootPath,
        string searchText,
        int skippedFolderCount,
        CancellationToken cancellationToken)
    {
        EnsureDatabase();
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        if (_supportsTrigramSearch && searchText.Length >= 3)
        {
            command.CommandText = """
                SELECT FolderPath
                FROM FolderNameSearch
                WHERE FolderNameSearch MATCH $query AND DriveRoot = $root
                ORDER BY FolderPath COLLATE NOCASE;
                """;
            command.Parameters.AddWithValue(
                "$query",
                $""{searchText.Replace(""", """")}"");
        }
        else
        {
            command.CommandText = """
                SELECT FolderPath
                FROM FolderEntries
                WHERE DriveRoot = $root AND FolderName LIKE $pattern ESCAPE '\'
                ORDER BY FolderPath COLLATE NOCASE;
                """;
            var escapedSearchText = searchText
                .Replace("\", "\\")
                .Replace("%", "\%")
                .Replace("_", "\_");
            command.Parameters.AddWithValue("$pattern", $"%{escapedSearchText}%");
        }

        command.Parameters.AddWithValue("$root", rootPath);
        var matches = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();
            matches.Add(reader.GetString(0));
        }

        return new FolderSearchResult(matches, skippedFolderCount);
    }

    private bool TryReadCurrentIndex(
        string rootPath,
        DriveFingerprint fingerprint,
        out FolderIndexSummary? summary)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT VolumeLabel, DriveFormat, DriveType, TotalSize, FolderCount, SkippedFolderCount, IndexedAtUtc
            FROM FolderDriveIndexes
            WHERE DriveRoot = $root;
            """;
        command.Parameters.AddWithValue("$root", rootPath);
        using var reader = command.ExecuteReader();

        if (!reader.Read() ||
            !string.Equals(reader.GetString(0), fingerprint.VolumeLabel, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(reader.GetString(1), fingerprint.DriveFormat, StringComparison.OrdinalIgnoreCase) ||
            reader.GetInt32(2) != (int)fingerprint.DriveType ||
            reader.GetInt64(3) != fingerprint.TotalSize)
        {
            summary = null;
            return false;
        }

        summary = new FolderIndexSummary(
            rootPath,
            reader.GetInt32(4),
            reader.GetInt32(5),
            DateTime.Parse(reader.GetString(6), null, System.Globalization.DateTimeStyles.RoundtripKind));
        return true;
    }

    private void EnsureDatabase()
    {
        if (_schemaInitialized)
            return;

        lock (_schemaLock)
        {
            if (_schemaInitialized)
                return;

            var dataSource = new SqliteConnectionStringBuilder(_connectionString).DataSource;
            Directory.CreateDirectory(Path.GetDirectoryName(dataSource)!);

            using var connection = OpenConnection();
            using (var journalMode = connection.CreateCommand())
            {
                journalMode.CommandText = "PRAGMA journal_mode = WAL;";
                journalMode.ExecuteScalar();
            }

            using (var schema = connection.CreateCommand())
            {
                schema.CommandText = """
                    CREATE TABLE IF NOT EXISTS FolderEntries (
                        DriveRoot TEXT NOT NULL,
                        FolderPath TEXT NOT NULL,
                        FolderName TEXT NOT NULL COLLATE NOCASE,
                        PRIMARY KEY (DriveRoot, FolderPath)
                    );
                    CREATE INDEX IF NOT EXISTS IX_FolderEntries_DriveRoot
                        ON FolderEntries (DriveRoot);
                    CREATE TABLE IF NOT EXISTS FolderDriveIndexes (
                        DriveRoot TEXT PRIMARY KEY,
                        VolumeLabel TEXT NOT NULL,
                        DriveFormat TEXT NOT NULL,
                        DriveType INTEGER NOT NULL,
                        TotalSize INTEGER NOT NULL,
                        FolderCount INTEGER NOT NULL,
                        SkippedFolderCount INTEGER NOT NULL,
                        IndexedAtUtc TEXT NOT NULL
                    );
                    """;
                schema.ExecuteNonQuery();
            }

            try
            {
                using var trigram = connection.CreateCommand();
                trigram.CommandText = """
                    CREATE VIRTUAL TABLE IF NOT EXISTS FolderNameSearch USING fts5(
                        FolderName,
                        DriveRoot UNINDEXED,
                        FolderPath UNINDEXED,
                        tokenize='trigram'
                    );
                    """;
                trigram.ExecuteNonQuery();
                _supportsTrigramSearch = true;
            }
            catch (SqliteException)
            {
                // Keep a correct LIKE-based fallback for SQLite builds without the trigram tokenizer.
                _supportsTrigramSearch = false;
            }

            _schemaInitialized = true;
        }
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static bool IsFileSystemAccessException(Exception exception) =>
        exception is UnauthorizedAccessException or IOException or SecurityException;

    private sealed record DriveFingerprint(
        string VolumeLabel,
        string DriveFormat,
        DriveType DriveType,
        long TotalSize);
}

internal sealed record FolderSearchResult(IReadOnlyList<string> Folders, int SkippedFolderCount);

internal sealed record FolderIndexSummary(
    string RootPath,
    int FolderCount,
    int SkippedFolderCount,
    DateTime IndexedAtUtc);

internal sealed record FolderWalkerIndexProgress(
    string RootPath,
    int FolderCount,
    int SkippedFolderCount);
