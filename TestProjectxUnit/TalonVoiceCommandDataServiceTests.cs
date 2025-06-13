using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        }
    }
}
