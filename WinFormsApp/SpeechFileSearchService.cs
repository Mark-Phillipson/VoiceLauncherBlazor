using System.Security;

namespace WinFormsApp;

/// <summary>
/// Searches for files by name in a selected folder, optionally including subfolders.
/// Inaccessible directories are skipped so one protected folder does not stop the search.
/// </summary>
internal sealed class SpeechFileSearchService
{
    public Task<FileSearchResult> SearchAsync(
        string rootPath,
        string searchText,
        bool includeSubfolders,
        bool sortAlphabetically,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"The selected folder does not exist: {rootPath}");

        return Task.Run(
            () => Search(rootPath, searchText?.Trim() ?? string.Empty, includeSubfolders, sortAlphabetically, cancellationToken),
            cancellationToken);
    }

    private static FileSearchResult Search(
        string rootPath,
        string searchText,
        bool includeSubfolders,
        bool sortAlphabetically,
        CancellationToken cancellationToken)
    {
        var matches = new List<FileSearchMatch>();
        var pendingFolders = new Stack<string>();
        var skippedFolderCount = 0;
        pendingFolders.Push(rootPath);

        while (pendingFolders.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentPath = pendingFolders.Pop();

            try
            {
                foreach (var filePath in Directory.EnumerateFiles(currentPath))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var fileName = Path.GetFileName(filePath);
                        if (!fileName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                            continue;

                        matches.Add(new FileSearchMatch(filePath, File.GetLastWriteTimeUtc(filePath)));
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

            if (!includeSubfolders)
                continue;

            try
            {
                foreach (var childPath in Directory.EnumerateDirectories(currentPath))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var attributes = File.GetAttributes(childPath);
                        if ((attributes & FileAttributes.ReparsePoint) == 0)
                            pendingFolders.Push(childPath);
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

        var orderedMatches = sortAlphabetically
            ? matches.OrderBy(match => Path.GetFileName(match.FullPath), StringComparer.OrdinalIgnoreCase)
            : matches.OrderByDescending(match => match.LastModifiedUtc);

        return new FileSearchResult(orderedMatches.ToArray(), skippedFolderCount);
    }

    private static bool IsFileSystemAccessException(Exception exception) =>
        exception is UnauthorizedAccessException or IOException or SecurityException;
}

internal sealed record FileSearchMatch(string FullPath, DateTime LastModifiedUtc);

internal sealed record FileSearchResult(IReadOnlyList<FileSearchMatch> Files, int SkippedFolderCount);
