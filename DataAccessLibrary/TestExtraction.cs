using System;
using System.IO;

namespace DataAccessLibrary
{
    public class SimpleExtractionTester
    {
        public static void Main(string[] args)
        {
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
                Console.WriteLine($"Testing: {path}");
                var result = ExtractRepositoryFromPath(path);
                Console.WriteLine($"RESULT: {result ?? "NULL"}");
                Console.WriteLine($"{'=',50}");
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
                            Console.WriteLine($"Repository found: {pathParts[1]}");
                            return pathParts[1];
                        }
                        else
                        {
                            Console.WriteLine($"Not enough path parts or empty repository name. Parts count: {pathParts.Length}");
                        }
                    }
                }
                
                Console.WriteLine("No user pattern found in path");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }
    }
}
