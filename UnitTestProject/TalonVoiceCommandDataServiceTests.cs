using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTestProject
{
    public class TalonVoiceCommandDataServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options, null!);
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
                "- application: vscode, mode: command",
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
    }
}
