using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Test cases
        var testPaths = new[]
        {
            @"C:\Users\MPhil\VoiceLauncherBlazor\talon\app.talon",
            @"C:\Users\MPhil\source\repos\VoiceLauncherBlazor\talon\app.talon",
            @"C:\Users\MPhil\MyProject\config.talon",
            @"/Users/john/ProjectName/talon/commands.talon",
            @"/home/user/AnotherRepo/scripts/test.talon"
        };

        foreach (var path in testPaths)
        {
            Console.WriteLine($"\n{'=',50}");
            var result = ExtractRepositoryFromPath(path);
            Console.WriteLine($"Path: {path}");
            Console.WriteLine($"RESULT: {result ?? "NULL"}");
            Console.WriteLine($"{'=',50}");
        }
    }

    static string? ExtractRepositoryFromPath(string filePath)
    {
        try
        {
            Console.WriteLine($"Input path: {filePath}");
            var normalizedPath = Path.GetFullPath(filePath).Replace('\\', '/');
            Console.WriteLine($"Normalized path: {normalizedPath}");
            
            // Look for common patterns that indicate a user directory
            var userPatterns = new[] { "/Users/", "/home/", "C:/Users/" };
            
            foreach (var pattern in userPatterns)
            {
                Console.WriteLine($"Checking pattern: {pattern}");
                var userIndex = normalizedPath.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"Pattern found at index: {userIndex}");
                
                if (userIndex >= 0)
                {
                    // Find the path after the user directory
                    var userDirStart = userIndex + pattern.Length;
                    var pathAfterUser = normalizedPath.Substring(userDirStart);
                    Console.WriteLine($"Path after user: {pathAfterUser}");
                    
                    // Split the path and get the parts
                    var pathParts = pathAfterUser.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine($"Path parts: [{string.Join(", ", pathParts)}]");
                    
                    // The first directory after the user directory is the repository
                    // Format: /Users/MPhil/VoiceLauncherBlazor/... 
                    // pathParts would be: ["MPhil", "VoiceLauncherBlazor", ...]
                    // We want the second part (index 1) which is the repository
                    
                    if (pathParts.Length >= 2 && !string.IsNullOrWhiteSpace(pathParts[1]))
                    {
                        Console.WriteLine($"Found repository: {pathParts[1]}");
                        return pathParts[1];
                    }
                    else
                    {
                        Console.WriteLine($"Not enough path parts or second part is empty. Length: {pathParts.Length}");
                        if (pathParts.Length > 0)
                        {
                            for (int i = 0; i < pathParts.Length; i++)
                            {
                                Console.WriteLine($"  Part {i}: '{pathParts[i]}'");
                            }
                        }
                    }
                }
            }
            
            Console.WriteLine("No repository found");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }
}
