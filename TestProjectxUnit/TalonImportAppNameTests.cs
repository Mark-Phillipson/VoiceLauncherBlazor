using Microsoft.EntityFrameworkCore;
using Xunit;
using DataAccessLibrary;
using DataAccessLibrary.Services;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;

namespace TestProjectxUnit
{
    public class TalonImportAppNameTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var config = new ConfigurationBuilder().Build(); // Provides an empty configuration
            var context = new ApplicationDbContext(options, config);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task ImportTalonFileContentAsync_WithAppNameFilter_ShouldParseApplicationCorrectly()
        {
            // Arrange
            var fileContent = @"os: windows
and app.name: ThereCameanEcho.exe
-

settings():
    user.mode_indicator_show = 0
    user.mouse_enable_pop_click = 0
    key_hold = 50
<user.screen_step_two_commandconquer> <user.screen_step_vertical>:
        mouse_move(screen_step_two_commandconquer, screen_step_vertical)
game <user.arrow_key>:
    key(arrow_key)
    repeat(4)
fly <user.arrow_key>:
    key(arrow_key)
    repeat(4)
    sleep(50ms)
    key(arrow_key)
    repeat(4)
    sleep(50ms)
    key(arrow_key)
    repeat(4)
    sleep(50ms)
zoom: key(keypad_8)
zoom out: key(keypad_2)
centre:
    mouse_move(800, 500)";

            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            var fileName = "there_came_an_echo.talon";

            // Act
            var count = await service.ImportTalonFileContentAsync(fileContent, fileName);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            
            // Should import the commands (excluding settings())
            Assert.True(count > 0, "Should import at least one command");
            Assert.True(commands.Count > 0, "Commands should be saved to database");
            
            // All commands should have the correct application name
            foreach (var command in commands)
            {
                Assert.Equal("ThereCameanEcho.exe", command.Application);
                Assert.Equal("windows", command.OperatingSystem);
            }
            
            // Should find specific commands
            Assert.Contains(commands, c => c.Command.Contains("game") && c.Application == "ThereCameanEcho.exe");
            Assert.Contains(commands, c => c.Command.Contains("zoom") && c.Application == "ThereCameanEcho.exe");
            Assert.Contains(commands, c => c.Command.Contains("centre") && c.Application == "ThereCameanEcho.exe");
        }        [Fact]
        public async Task ImportTalonFileContentAsync_WithComplexAndCondition_ShouldWorkNow()
        {
            // Arrange - This test verifies the fix is working
            var fileContent = @"os: windows
and app.name: ThereCameanEcho.exe
-
test command: key(a)";

            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            var fileName = "test.talon";

            // Act
            var count = await service.ImportTalonFileContentAsync(fileContent, fileName);

            // Assert - Now this should work correctly
            var commands = dbContext.TalonVoiceCommands.ToList();
            
            Assert.True(commands.Any(), "Should import at least one command");
            
            var firstCommand = commands.First();
              // This assertion should now PASS with the fixed implementation
            Assert.Equal("ThereCameanEcho.exe", firstCommand.Application);
            Assert.Equal("windows", firstCommand.OperatingSystem);
        }

        [Fact]
        public async Task ImportTalonFileContentAsync_WithVariousAppNamePatterns_ShouldParseCorrectly()
        {
            // Test different patterns of app name specification
            var testCases = new[]
            {
                new { Content = "app: notepad\n-\ntest: key(a)", Expected = "notepad" },
                new { Content = "application: chrome\n-\ntest: key(a)", Expected = "chrome" },
                new { Content = "app.exe: colobot.exe\n-\ntest: key(a)", Expected = "colobot.exe" },
                new { Content = "and app.name: firefox.exe\n-\ntest: key(a)", Expected = "firefox.exe" },
                new { Content = "os: windows\nand app.name: Visual Studio Code\n-\ntest: key(a)", Expected = "Visual Studio Code" },
                new { Content = "mode: command\nand app.name: /Some/Path/app.exe\n-\ntest: key(a)", Expected = "/Some/Path/app.exe" }
            };

            foreach (var testCase in testCases)
            {
                // Arrange
                var dbContext = GetInMemoryDbContext();
                var service = new TalonVoiceCommandDataService(dbContext);
                var fileName = "test.talon";

                // Act
                var count = await service.ImportTalonFileContentAsync(testCase.Content, fileName);

                // Assert
                var commands = dbContext.TalonVoiceCommands.ToList();
                Assert.True(commands.Any(), $"Should import at least one command for pattern: {testCase.Content}");
                
                var firstCommand = commands.First();                Assert.Equal(testCase.Expected, firstCommand.Application);
            }
        }
        [Fact]
        public async Task ImportTalonFileContentAsync_WithRealUserFile_ShouldParseCorrectly()
        {
            // This uses the exact content from the user's attachment that was failing
            var fileContent = @"os: windows
and app.name: ThereCameanEcho.exe
-

settings():
    user.mode_indicator_show = 0
    user.mouse_enable_pop_click = 0
    key_hold = 50
<user.screen_step_two_commandconquer> <user.screen_step_vertical>:
        mouse_move(screen_step_two_commandconquer, screen_step_vertical)
game <user.arrow_key>:
    key(arrow_key)
    repeat(4)
fly <user.arrow_key>:
    key(arrow_key)
    repeat(4)
    sleep(50ms)
    key(arrow_key)
    repeat(4)
    sleep(50ms)
    key(arrow_key)
    repeat(4)
    sleep(50ms)
zoom: key(keypad_8)
zoom out: key(keypad_2)
centre:
    mouse_move(800, 500)";

            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            var fileName = "there_came_an_echo.talon";

            // Act
            var count = await service.ImportTalonFileContentAsync(fileContent, fileName);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            
            // Should import the commands (excluding settings())
            Assert.True(count > 0, "Should import at least one command");
            Assert.True(commands.Count > 0, "Commands should be saved to database");
            
            // All commands should have the correct application name and OS
            foreach (var command in commands)
            {
                Assert.Equal("ThereCameanEcho.exe", command.Application);
                Assert.Equal("windows", command.OperatingSystem);
            }
            
            // Should find specific commands and they should all have the correct app
            var gameCommand = commands.FirstOrDefault(c => c.Command.Contains("game"));
            Assert.NotNull(gameCommand);
            Assert.Equal("ThereCameanEcho.exe", gameCommand.Application);
            
            var zoomCommand = commands.FirstOrDefault(c => c.Command.Contains("zoom"));
            Assert.NotNull(zoomCommand);
            Assert.Equal("ThereCameanEcho.exe", zoomCommand.Application);
            
            var centreCommand = commands.FirstOrDefault(c => c.Command.Contains("centre"));
            Assert.NotNull(centreCommand);
            Assert.Equal("ThereCameanEcho.exe", centreCommand.Application);
        }
        [Fact]
        public async Task ImportTalonFileContentAsync_WithAppExePattern_ShouldParseCorrectly()
        {
            // Arrange - This tests the app.exe: pattern from the user's current file
            var fileContent = @"app.exe: colobot.exe
-

# Colobot
# Colobot is a game that teaches programming in C++ and JavaScript. It is a game where you control robots to complete tasks.

settings():
    user.mode_indicator_show = 0
    # Choose how pop click should work in 'control mouse' mode
    # 0 = off
    # 1 = on with eyetracker but not zoom mouse mode
    # 2 = on but not with zoom mouse mode
    user.mouse_enable_pop_click = 0
    key_hold = 32
    # Zoom Mouse Options     
    tracking.zoom_live = true
    tracking.zoom_height = 300
    tracking.zoom_width = 300
    tracking.zoom_scale = 4
    

game <user.arrow_key>:
    key(arrow_key)
    repeat(4)
fly <user.arrow_key>:
    key(arrow_key)
    repeat(7)
    sleep(50ms)
    key(arrow_key)
    repeat(7)
    sleep(50ms)
# Basic commands
move forward:
    key(up)
move back:
    key(down)
move left:
    key(left)
move right:
    key(right)
takeoff: key(shift)
descend: key(ctrl)
main action: key(e)
astronaut: key(home)
previous: key(keypad_0)
camera [toggle]: key(space)
zoom [in]: key(keypad_plus)
zoom out: key(keypad_minus)
pause [game]: key(keypad_decimal)
mission [instructions]: key(f1)
programming [help]: key(f2)
slowdown: key(f6)
normal [speed]: key(f7)
fast [speed]: key(f8)
save [game]: key(f5)
load [game]: key(f9)
exit [mission]: key(escape)";

            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            var fileName = "colobot.talon";

            // Act
            var count = await service.ImportTalonFileContentAsync(fileContent, fileName);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            
            Assert.True(count > 0, "Should import at least one command");
            Assert.True(commands.Count > 0, "Commands should be saved to database");
            
            // All commands should have the correct application name
            foreach (var command in commands)
            {
                Assert.Equal("colobot.exe", command.Application);
            }
              // Should find specific commands and they should all have the correct app
            var moveForwardCommand = commands.FirstOrDefault(c => c.Command.Contains("move forward"));
            Assert.NotNull(moveForwardCommand);
            Assert.Equal("colobot.exe", moveForwardCommand.Application);
            
            var takeoffCommand = commands.FirstOrDefault(c => c.Command.Contains("takeoff"));
            Assert.NotNull(takeoffCommand);
            Assert.Equal("colobot.exe", takeoffCommand.Application);
        }
        [Fact]
        public async Task ImportTalonFileContentAsync_WithActualUserFile_ShouldParseCorrectly()
        {
            // This uses the exact content from the user's current file
            var fileContent = @"app.exe: colobot.exe
-

# Colobot
# Colobot is a game that teaches programming in C++ and JavaScript. It is a game where you control robots to complete tasks.

settings():
    user.mode_indicator_show = 0
    # Choose how pop click should work in 'control mouse' mode
    # 0 = off
    # 1 = on with eyetracker but not zoom mouse mode
    # 2 = on but not with zoom mouse mode
    user.mouse_enable_pop_click = 0
    key_hold = 32
    # Zoom Mouse Options     
    tracking.zoom_live = true
    tracking.zoom_height = 300
    tracking.zoom_width = 300
    tracking.zoom_scale = 4
    

game <user.arrow_key>:
    key(arrow_key)
    repeat(4)
fly <user.arrow_key>:
    key(arrow_key)
    repeat(7)
    sleep(50ms)
    key(arrow_key)
    repeat(7)
    sleep(50ms)
# Basic commands
move forward:
    key(up)
move back:
    key(down)
move left:
    key(left)
move right:
    key(right)
takeoff: key(shift)
descend: key(ctrl)
main action: key(e)
astronaut: key(home)
previous: key(keypad_0)
camera [toggle]: key(space)
zoom [in]: key(keypad_plus)
zoom out: key(keypad_minus)
pause [game]: key(keypad_decimal)
mission [instructions]: key(f1)
programming [help]: key(f2)
slowdown: key(f6)
normal [speed]: key(f7)
fast [speed]: key(f8)
save [game]: key(f5)
load [game]: key(f9)
exit [mission]: key(escape)";

            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            var fileName = "colobot.talon";

            // Act
            var count = await service.ImportTalonFileContentAsync(fileContent, fileName);

            // Assert
            var commands = dbContext.TalonVoiceCommands.ToList();
            
            // Should import the commands (excluding settings())
            Assert.True(count > 0, "Should import at least one command");
            Assert.True(commands.Count > 0, "Commands should be saved to database");
            
            // All commands should have the correct application name
            foreach (var command in commands)
            {
                Assert.Equal("colobot.exe", command.Application);
            }
            
            // Should find specific commands and they should all have the correct app
            var gameCommand = commands.FirstOrDefault(c => c.Command.Contains("game"));
            Assert.NotNull(gameCommand);
            Assert.Equal("colobot.exe", gameCommand.Application);
            
            var moveForwardCommand = commands.FirstOrDefault(c => c.Command.Contains("move forward"));
            Assert.NotNull(moveForwardCommand);
            Assert.Equal("colobot.exe", moveForwardCommand.Application);
            
            var zoomCommand = commands.FirstOrDefault(c => c.Command.Contains("zoom"));
            Assert.NotNull(zoomCommand);
            Assert.Equal("colobot.exe", zoomCommand.Application);
        }
    }
}
