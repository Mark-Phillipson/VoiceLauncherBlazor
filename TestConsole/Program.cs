using System;
using System.IO;

class Program
{
    static void Main()
    {
        var testPaths = new[]
        {
            "invalidpath",
            "C:/Users/MPhil//file.txt",
            "C:/Users/MPhil/VoiceLauncherBlazor/talon/app.talon",
            "/Users/MPhil/VoiceLauncherBlazor/talon/app.talon"
        };

        foreach (var path in testPaths)
        {
            var result = ExtractRepositoryFromPath(path);
            Console.WriteLine($"Path: '{path}' -> Repository: '{result}'");
            
            // Let's also see the full path to understand what's happening
            try
            {
                var fullPath = Path.GetFullPath(path);
                Console.WriteLine($"  Full path: '{fullPath}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error getting full path: {ex.Message}");
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Extracts the repository name from a file path by finding the first subdirectory after the user's directory
    /// For example: C:\Users\MPhil\VoiceLauncherBlazor\talon\app.talon -> VoiceLauncherBlazor
    /// </summary>
    private static string? ExtractRepositoryFromPath(string filePath)
    {
        try
        {
            var normalizedPath = Path.GetFullPath(filePath).Replace('\\', '/');
            Console.WriteLine($"  Normalized path: '{normalizedPath}'");
            
            // Look for common patterns that indicate a user directory
            var userPatterns = new[] { "/Users/", "/home/", "C:/Users/" };
            
            foreach (var pattern in userPatterns)
            {
                var userIndex = normalizedPath.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (userIndex >= 0)
                {
                    Console.WriteLine($"  Found pattern '{pattern}' at index {userIndex}");
                    // Find the path after the user directory
                    var userDirStart = userIndex + pattern.Length;
                    var pathAfterUser = normalizedPath.Substring(userDirStart);
                    Console.WriteLine($"  Path after user: '{pathAfterUser}'");
                    
                    // Split the path and get the parts
                    var pathParts = pathAfterUser.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine($"  Path parts: [{string.Join(", ", pathParts)}]");
                    
                    // The first directory after the user directory is the repository
                    // Format: /Users/MPhil/VoiceLauncherBlazor/... 
                    // pathParts would be: ["MPhil", "VoiceLauncherBlazor", ...]
                    // We want the second part (index 1) which is the repository
                    
                    if (pathParts.Length >= 2 && !string.IsNullOrWhiteSpace(pathParts[1]))
                    {
                        Console.WriteLine($"  Repository found: {pathParts[1]}");
                        return pathParts[1];
                    }
                    else
                    {
                        Console.WriteLine($"  Not enough path parts or empty repository name. Parts count: {pathParts.Length}");
                        break;
                    }
                }
            }
            
            Console.WriteLine("  No user pattern found");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Exception: {ex.Message}");
            return null;
        }
    }
}
