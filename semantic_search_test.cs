using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TestSemanticSearch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create an in-memory database context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var config = new ConfigurationBuilder().Build();
            var dbContext = new ApplicationDbContext(options, config);
            var service = new TalonVoiceCommandDataService(dbContext);

            // Add test commands
            var commands = new[]
            {
                new TalonVoiceCommand 
                { 
                    Command = "copy line", 
                    Script = "user.copy_current_line()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "Copy Operations",
                    FilePath = "/test/copy.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "copy selection", 
                    Script = "user.copy_selection()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "Copy Operations",
                    FilePath = "/test/copy.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "duplicate line", 
                    Script = "user.duplicate_current_line()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "Line Operations",
                    FilePath = "/test/line.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "open file", 
                    Script = "user.open_file()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "File Operations",
                    FilePath = "/test/file.talon"
                }
            };
            
            await dbContext.TalonVoiceCommands.AddRangeAsync(commands);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("Testing semantic search for 'duplicate' should find 'copy' commands...");
            
            // Test semantic search
            var results = await service.SemanticSearchAsync("duplicate");
            
            Console.WriteLine($"Found {results.Count} results for 'duplicate':");
            foreach (var result in results)
            {
                Console.WriteLine($"  - Command: {result.Command}");
                Console.WriteLine($"    Script: {result.Script}");
                Console.WriteLine($"    Title: {result.Title}");
                Console.WriteLine();
            }

            // Test with semantic search with lists
            Console.WriteLine("Testing semantic search with lists for 'duplicate'...");
            var listResults = await service.SemanticSearchWithListsAsync("duplicate");
            
            Console.WriteLine($"Found {listResults.Count} results for 'duplicate' with lists:");
            foreach (var result in listResults)
            {
                Console.WriteLine($"  - Command: {result.Command}");
                Console.WriteLine($"    Script: {result.Script}");
                Console.WriteLine($"    Title: {result.Title}");
                Console.WriteLine();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
