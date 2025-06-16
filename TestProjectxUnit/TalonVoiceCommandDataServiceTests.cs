using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace TestProjectxUnit
{
    public class TalonVoiceCommandDataServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var config = new ConfigurationBuilder().Build(); // Provides an empty configuration
            return new ApplicationDbContext(options, config);
        }

        // Helper method to access the private ExtractRepositoryFromPath method using reflection
        private string? CallExtractRepositoryFromPath(TalonVoiceCommandDataService service, string filePath)
        {
            var methodInfo = typeof(TalonVoiceCommandDataService).GetMethod("ExtractRepositoryFromPath", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            return (string?)methodInfo?.Invoke(service, new object[] { filePath });
        }

        [Fact]
        public async Task ImportFromTalonFilesAsync_ParsesTalonFilesAndSavesCommands()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var talonFile = Path.Combine(tempDir, "test.talon");
            await File.WriteAllLinesAsync(talonFile, new[]
            {
                "application: vscode",
                "mode: command",
                "-",
                "open file: user.open_file()",
                "# comment line",
                "save file: user.save_file()"
            });
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var count = await service.ImportFromTalonFilesAsync(tempDir);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            Assert.Equal(2, commands.Count);
            Assert.Contains(commands, c => c.Command == "open file" && c.Script == "user.open_file()" && c.Application == "vscode" && c.Mode == "command");
            Assert.Contains(commands, c => c.Command == "save file" && c.Script == "user.save_file()" && c.Application == "vscode" && c.Mode == "command");

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task ImportFromTalonFilesAsync_ParsesTalonFilesAndSavesCommands_FromRealFile()
        {
            // Arrange
            var talonFile = Path.Combine(Directory.GetCurrentDirectory(), "test.talon");
            var tempDir = Path.GetDirectoryName(talonFile)!;
            // Create the test.talon file with required contents
            await File.WriteAllLinesAsync(talonFile, new[]
            {
                "application: vscode",
                "mode: command",
                "-",
                "error lens toggle: user.vscode(\"errorLens.toggle\")",
                "T4 for inject: insert(\"<#= #>\")"
            });
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var count = await service.ImportFromTalonFilesAsync(tempDir);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            Assert.NotEmpty(commands); // At least one command should be imported
            Assert.Contains(commands, c => c.Command == "error lens toggle" && c.Script == "user.vscode(\"errorLens.toggle\")");
            Assert.Contains(commands, c => c.Command == "T4 for inject" && c.Script.Contains("insert(\"<#= #>\")"));

            // Cleanup
            if (File.Exists(talonFile))
                File.Delete(talonFile);
        }

        [Fact]
        public async Task ImportFromTalonFilesAsync_IgnoresSettingsAndImportsCommands()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var talonFile = Path.Combine(tempDir, "test.talon");
            await File.WriteAllLinesAsync(talonFile, new[]
            {
                "app.name: Company of Heroes 2",
                "-",
                "settings():",
                "    user.mode_indicator_show = 0",
                "    user.mouse_enable_pop_click = 0",
                "    key_hold = 32",
                "    tracking.zoom_live = true",
                "    tracking.zoom_height = 300",
                "    tracking.zoom_width = 300",
                "    tracking.zoom_scale = 4",
                "",
                "# This is a list of the commands that are specific to the Company of Heroes 2 game",
                "game <user.arrow_key>:",
                "    key(arrow_key)",
                "    repeat(4)",
                "fly <user.arrow_key>:",
                "    key(arrow_key)",
                "    repeat(7)",
                "    sleep(50ms)",
                "    key(arrow_key)",
                "    repeat(7)",
                "    sleep(50ms)",
                "move forward:",
                "    key(up)",
                "move back:",
                "    key(down)"
            });
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var count = await service.ImportFromTalonFilesAsync(tempDir);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            Assert.Contains(commands, c => c.Command == "game <user.arrow_key>" && c.Script.Contains("key(arrow_key)"));
            Assert.Contains(commands, c => c.Command == "fly <user.arrow_key>" && c.Script.Contains("repeat(7)"));
            Assert.Contains(commands, c => c.Command == "move forward" && c.Script.Contains("key(up)"));
            Assert.Contains(commands, c => c.Command == "move back" && c.Script.Contains("key(down)"));
            Assert.DoesNotContain(commands, c => c.Command == "settings()");

            // Cleanup
            Directory.Delete(tempDir, true);
        }        [Theory]
        [InlineData("C:/Users/MPhil/AppData/Roaming/talon/user/community/file.talon", "community")]
        [InlineData("C:\\Users\\MPhil\\AppData\\Roaming\\talon\\user\\community\\subfolder\\app.talon", "community")]
        [InlineData("/home/jane/.talon_user/user/my-repo/config.py", "my-repo")]
        [InlineData("C:/some/path/user/project123/scripts/main.talon", "project123")]
        [InlineData("/Users/developer/talon/user/awesome-project/commands.talon", "awesome-project")]
        [InlineData("C:\\talon\\user\\some-repo-name\\folder\\file.txt", "some-repo-name")]
        public void ExtractRepositoryFromPath_ValidPaths_ReturnsCorrectRepository(string filePath, string expectedRepository)
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var result = CallExtractRepositoryFromPath(service, filePath);

            // Assert
            Assert.Equal(expectedRepository, result);
        }[Theory]
        [InlineData("C:/Users/MPhil/")]
        [InlineData("C:\\Users\\MPhil")]
        [InlineData("/Users/MPhil")]
        [InlineData("C:/Users/")]
        [InlineData("/Users/")]
        [InlineData("")]
        [InlineData("C:/InvalidPath/file.txt")]
        [InlineData("D:/SomeOtherPath/file.txt")]
        [InlineData("C:/talon/notuser/community/file.talon")] // wrong directory name
        [InlineData("C:/some/path/withoutuser/directory.txt")] // no user directory
        public void ExtractRepositoryFromPath_InvalidPaths_ReturnsNull(string filePath)
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var result = CallExtractRepositoryFromPath(service, filePath);

            // Assert
            Assert.Null(result);
        }[Theory]
        [InlineData("C:/Users/MPhil/AppData/Roaming/talon/user/community/subfolder/deep/file.talon", "community")]
        [InlineData("/home/developer/talon/user/awesome-project/src/components/Button.talon", "awesome-project")]
        [InlineData("C:\\some\\path\\user\\my_project\\tests\\unit\\test_file.py", "my_project")]
        public void ExtractRepositoryFromPath_DeepPaths_ReturnsCorrectRepository(string filePath, string expectedRepository)
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var result = CallExtractRepositoryFromPath(service, filePath);

            // Assert
            Assert.Equal(expectedRepository, result);
        }        [Theory]
        [InlineData("C:/some/path/user/ /file.txt")]
        [InlineData("C:/some/path/user/   /file.txt")]
        public void ExtractRepositoryFromPath_EmptyOrWhitespaceRepository_ReturnsNull(string filePath)
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var result = CallExtractRepositoryFromPath(service, filePath);

            // Assert
            Assert.Null(result);
        }[Theory]
        [InlineData("C:/some/path/user/community", "community")] // Direct repository directory
        [InlineData("/talon/user/my-repo", "my-repo")] // Simple path
        public void ExtractRepositoryFromPath_RelativeAndDirectPaths_ReturnsExpectedResult(string filePath, string expectedRepository)
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var result = CallExtractRepositoryFromPath(service, filePath);

            // Assert
            Assert.Equal(expectedRepository, result);
        }[Theory]
        [InlineData("C:/some/path/user//file.txt", "file.txt")] // Double slash creates a "repository" name
        [InlineData("C:/some/path/without/user/directory.txt", "directory.txt")] // Contains "/user/" substring
        public void ExtractRepositoryFromPath_EdgeCases_ReturnsUnexpectedButValidResult(string filePath, string expectedRepository)
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var result = CallExtractRepositoryFromPath(service, filePath);

            // Assert
            Assert.Equal(expectedRepository, result);
        }

        [Fact]
        public async Task ImportFromTalonFilesAsync_ParsesTitleFromHeader_SavesCommandsWithTitle()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var talonFile = Path.Combine(tempDir, "test_with_title.talon");
            await File.WriteAllLinesAsync(talonFile, new[]
            {
                "app: vscode",
                "title: My Custom Title",
                "mode: command",
                "-",
                "open file: user.open_file()",
                "save file: user.save_file()"
            });
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var count = await service.ImportFromTalonFilesAsync(tempDir);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            Assert.Equal(2, count);
            Assert.Equal(2, commands.Count);
            
            // Verify both commands have the title from the header
            Assert.All(commands, command => 
            {
                Assert.Equal("My Custom Title", command.Title);
                Assert.Equal("vscode", command.Application);
                Assert.Equal("command", command.Mode);
            });
            
            // Verify specific commands
            Assert.Contains(commands, c => c.Command == "open file" && c.Script == "user.open_file()");
            Assert.Contains(commands, c => c.Command == "save file" && c.Script == "user.save_file()");

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task ImportTalonFileContentAsync_ParsesTitleFromHeader_SavesCommandsWithTitle()
        {
            // Arrange
            var fileContent = @"app: chrome
title: ChatGPT Shortcuts
mode: chat
-
new chat: 
    key(ctrl-shift-o)

focus input: 
    key(shift-esc)

copy code block: 
    key(ctrl-shift-;)";

            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            var fileName = "test_chatgpt.talon";

            // Act
            var count = await service.ImportTalonFileContentAsync(fileContent, fileName);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            Assert.Equal(3, count);
            Assert.Equal(3, commands.Count);
            
            // Verify all commands have the title from the header
            Assert.All(commands, command => 
            {
                Assert.Equal("ChatGPT Shortcuts", command.Title);
                Assert.Equal("chrome", command.Application);
                Assert.Equal("chat", command.Mode);
            });
            
            // Verify specific commands
            Assert.Contains(commands, c => c.Command == "new chat" && c.Script.Contains("key(ctrl-shift-o)"));
            Assert.Contains(commands, c => c.Command == "focus input" && c.Script.Contains("key(shift-esc)"));
            Assert.Contains(commands, c => c.Command == "copy code block" && c.Script.Contains("key(ctrl-shift-;)"));
        }

        [Fact]
        public async Task ImportFromTalonFilesAsync_NoTitleInHeader_SavesCommandsWithNullTitle()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var talonFile = Path.Combine(tempDir, "test_no_title.talon");
            await File.WriteAllLinesAsync(talonFile, new[]
            {
                "app: notepad",
                "mode: command",
                "-",
                "save file: key(ctrl-s)",
                "open file: key(ctrl-o)"
            });
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var count = await service.ImportFromTalonFilesAsync(tempDir);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            Assert.Equal(2, commands.Count);
            
            // Verify commands have null title when not specified in header
            Assert.All(commands, command => 
            {
                Assert.Null(command.Title);
                Assert.Equal("notepad", command.Application);
                Assert.Equal("command", command.Mode);
            });

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task ImportFromTalonFilesAsync_LongTitleTruncation_SavesCommandsWithTruncatedTitle()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var talonFile = Path.Combine(tempDir, "test_long_title.talon");
            
            // Create a title longer than 200 characters
            var longTitle = new string('A', 250); // 250 characters
            await File.WriteAllLinesAsync(talonFile, new[]
            {
                "app: test_app",
                $"title: {longTitle}",
                "-",
                "test command: key(ctrl-t)"
            });
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var count = await service.ImportFromTalonFilesAsync(tempDir);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            Assert.Single(commands);
            
            var command = commands.First();
            Assert.NotNull(command.Title);
            Assert.Equal(200, command.Title.Length); // Should be truncated to 200 characters
            Assert.Equal(new string('A', 200), command.Title); // Should be first 200 'A' characters

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task ImportFromTalonFilesAsync_NoHeaderSection_SavesCommandsWithNullTitle()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var talonFile = Path.Combine(tempDir, "test_no_header.talon");
            await File.WriteAllLinesAsync(talonFile, new[]
            {
                "# This file has no header section, commands start immediately",
                "global command: key(ctrl-g)",
                "another command: insert('Hello World')"
            });
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);

            // Act
            var count = await service.ImportFromTalonFilesAsync(tempDir);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            Assert.Equal(2, commands.Count);
            
            // Verify commands have null title and global application when no header section
            Assert.All(commands, command => 
            {
                Assert.Null(command.Title);
                Assert.Equal("global", command.Application);
                Assert.Null(command.Mode);
            });

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task SemanticSearchAsync_FindsCommandsByTitle()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
              // Add test commands with titles
            var commands = new[]
            {
                new TalonVoiceCommand 
                { 
                    Command = "open file", 
                    Script = "user.open_file()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "File Operations",
                    FilePath = "/test/file1.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "save document", 
                    Script = "user.save_document()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "Document Management",
                    FilePath = "/test/file2.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "close tab", 
                    Script = "user.close_tab()", 
                    Application = "browser", 
                    Mode = "command",
                    Title = "File Operations",
                    FilePath = "/test/file3.talon"
                }
            };
            
            await dbContext.TalonVoiceCommands.AddRangeAsync(commands);
            await dbContext.SaveChangesAsync();

            // Act - Search by title
            var results = await service.SemanticSearchAsync("File Operations");

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, cmd => Assert.Equal("File Operations", cmd.Title));
            Assert.Contains(results, cmd => cmd.Command == "open file");
            Assert.Contains(results, cmd => cmd.Command == "close tab");
        }

        [Fact]
        public async Task SemanticSearchAsync_FindsCommandsByPartialTitle()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
              var commands = new[]
            {
                new TalonVoiceCommand 
                { 
                    Command = "edit text", 
                    Script = "user.edit_text()", 
                    Application = "editor", 
                    Mode = "command",
                    Title = "Text Editing Commands",
                    FilePath = "/test/editing.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "format code", 
                    Script = "user.format_code()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "Code Formatting Tools",
                    FilePath = "/test/formatting.talon"
                }
            };
            
            await dbContext.TalonVoiceCommands.AddRangeAsync(commands);
            await dbContext.SaveChangesAsync();

            // Act - Search by partial title
            var results = await service.SemanticSearchAsync("editing");

            // Assert
            Assert.Single(results);
            Assert.Equal("Text Editing Commands", results.First().Title);
            Assert.Equal("edit text", results.First().Command);
        }

        [Fact]
        public async Task SemanticSearchAsync_CombinesTitleAndCommandSearch()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);            var commands = new[]
            {
                new TalonVoiceCommand 
                { 
                    Command = "open file dialog", 
                    Script = "user.open_file_dialog()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "Dialog Commands",
                    FilePath = "/test/dialogs.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "save project", 
                    Script = "user.save_project()", 
                    Application = "vscode", 
                    Mode = "command",
                    Title = "File Operations",
                    FilePath = "/test/project.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "find text", 
                    Script = "user.find_text()", 
                    Application = "editor", 
                    Mode = "command",
                    Title = "Search Tools",
                    FilePath = "/test/search.talon"
                }
            };
            
            await dbContext.TalonVoiceCommands.AddRangeAsync(commands);
            await dbContext.SaveChangesAsync();            // Act - Search term that matches both command and title
            var results = await service.SemanticSearchAsync("file");

            // Assert
            Assert.Equal(2, results.Count);
            // Should find both the command with "file" in the name and the one with "File Operations" in title
            Assert.Contains(results, cmd => cmd.Command == "open file dialog");
            Assert.Contains(results, cmd => cmd.Title == "File Operations");
        }

        [Fact]
        public async Task SemanticSearchWithListsAsync_IncludesTitleInSearch()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
              var command = new TalonVoiceCommand 
            { 
                Command = "navigate menu", 
                Script = "user.navigate_menu()", 
                Application = "app", 
                Mode = "command",
                Title = "Navigation Commands",
                FilePath = "/test/navigation.talon"
            };
            
            await dbContext.TalonVoiceCommands.AddAsync(command);
            await dbContext.SaveChangesAsync();

            // Act
            var results = await service.SemanticSearchWithListsAsync("navigation");

            // Assert
            Assert.Single(results);
            Assert.Equal("Navigation Commands", results.First().Title);
            Assert.Equal("navigate menu", results.First().Command);
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SemanticSearchAsync_HandlesEmptyTitleSearchTerms(string searchTerm)
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
              var command = new TalonVoiceCommand 
            { 
                Command = "test command", 
                Script = "user.test()", 
                Application = "test", 
                Mode = "command",
                Title = "Test Title",
                FilePath = "/test/test.talon"
            };
            
            await dbContext.TalonVoiceCommands.AddAsync(command);
            await dbContext.SaveChangesAsync();

            // Act
            var results = await service.SemanticSearchAsync(searchTerm);

            // Assert - Should return recent commands when search term is empty
            Assert.Single(results);
            Assert.Equal("test command", results.First().Command);
        }

        [Fact]
        public async Task SemanticSearchAsync_HandlesNullSearchTerm()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            
            var command = new TalonVoiceCommand 
            { 
                Command = "test command", 
                Script = "user.test()", 
                Application = "test", 
                Mode = "command",
                Title = "Test Title",
                FilePath = "/test/test.talon"
            };
            
            await dbContext.TalonVoiceCommands.AddAsync(command);
            await dbContext.SaveChangesAsync();

            // Act
            var results = await service.SemanticSearchAsync(null!);

            // Assert - Should return recent commands when search term is null
            Assert.Single(results);
            Assert.Equal("test command", results.First().Command);
        }

        [Fact]
        public async Task SemanticSearchAsync_IgnoresNullTitles()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
              var commands = new[]
            {
                new TalonVoiceCommand 
                { 
                    Command = "command with title", 
                    Script = "user.with_title()", 
                    Application = "app", 
                    Mode = "command",
                    Title = "Test Title",
                    FilePath = "/test/with_title.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "command without title", 
                    Script = "user.without_title()", 
                    Application = "app", 
                    Mode = "command",
                    Title = null,
                    FilePath = "/test/without_title.talon"
                }
            };
            
            await dbContext.TalonVoiceCommands.AddRangeAsync(commands);
            await dbContext.SaveChangesAsync();

            // Act - Search for something that would match null titles if not handled properly
            var results = await service.SemanticSearchAsync("test");

            // Assert - Should only find the command with actual title
            Assert.Single(results);
            Assert.Equal("command with title", results.First().Command);
            Assert.Equal("Test Title", results.First().Title);
        }

        [Fact]
        public async Task SemanticSearchAsync_CaseInsensitiveTitleSearch()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
              var command = new TalonVoiceCommand 
            { 
                Command = "test command", 
                Script = "user.test()", 
                Application = "app", 
                Mode = "command",
                Title = "Window Management Tools",
                FilePath = "/test/window.talon"
            };
            
            await dbContext.TalonVoiceCommands.AddAsync(command);
            await dbContext.SaveChangesAsync();

            // Act - Search with different case
            var results = await service.SemanticSearchAsync("WINDOW management");

            // Assert
            Assert.Single(results);
            Assert.Equal("Window Management Tools", results.First().Title);
        }
    }
}
