using System.Security;

namespace WinFormsApp;

/// <summary>
/// Finds folders whose names contain a search phrase without blocking the UI thread.
/// Reparse points are skipped so junctions and symbolic links cannot create traversal loops.
/// </summary>
internal sealed class FolderWalkerService
{
    public Task<FolderSearchResult> SearchAsync(
        string rootPath,
        string searchText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);

        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"The selected drive or folder does not exist: {rootPath}");

        var normalizedSearchText = searchText.Trim();
        if (normalizedSearchText.Length == 0)
            throw new ArgumentException("Enter a folder name to search for.", nameof(searchText));

        return Task.Run(() => Search(rootPath, normalizedSearchText, cancellationToken), cancellationToken);
    }

    private static FolderSearchResult Search(
        string rootPath,
        string searchText,
        CancellationToken cancellationToken)
    {
        var matches = new List<string>();
        var pendingFolders = new Stack<string>();
        var skippedFolderCount = 0;
        pendingFolders.Push(rootPath);

        while (pendingFolders.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentPath = pendingFolders.Pop();

            IEnumerable<string> children;
            try
            {
                children = Directory.EnumerateDirectories(currentPath);
            }
            catch (Exception exception) when (IsFileSystemAccessException(exception))
            {
                skippedFolderCount++;
                continue;
            }

            try
            {
                foreach (var childPath in children)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var attributes = File.GetAttributes(childPath);
                        if ((attributes & FileAttributes.ReparsePoint) != 0)
                            continue;

                        var folderName = Path.GetFileName(childPath.TrimEnd(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar));

                        if (folderName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                            matches.Add(childPath);

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
                // Enumeration can fail after returning some children. Keep those already found.
                skippedFolderCount++;
            }
        }

        matches.Sort(StringComparer.OrdinalIgnoreCase);
        return new FolderSearchResult(matches, skippedFolderCount);
    }

    private static bool IsFileSystemAccessException(Exception exception) =>
        exception is UnauthorizedAccessException or IOException or SecurityException;
}

internal sealed record FolderSearchResult(IReadOnlyList<string> Folders, int SkippedFolderCount);
