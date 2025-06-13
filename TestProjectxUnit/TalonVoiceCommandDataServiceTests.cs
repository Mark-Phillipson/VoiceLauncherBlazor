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
    }
}
