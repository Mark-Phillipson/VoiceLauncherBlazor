using RazorClassLibrary.Services;
using RazorClassLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace TestProjectxUnit
{
    public class TalonTagConflictTests
    {
        [Fact]
        public void CouldCommandsBeActiveSimultaneously_WithDifferentTags_ShouldReturnFalse()
        {
            // Arrange - Commands with completely different tag sets should not conflict
            var cmd1 = new TalonCommand
            {
                Command = "choose <number_small>",
                Tags = "community",
                Repository = "community"
            };

            var cmd2 = new TalonCommand
            {
                Command = "choose <number_small>",
                Tags = "user.command_wizard_showing,talon_ui_helper",
                Repository = "talon_ui_helper"
            };

            var cmd3 = new TalonCommand
            {
                Command = "choose <number_small>",
                Tags = "command.user.dictation_command,user.gaze_ocr_disambiguation,talon-gaze-ocr",                Repository = "talon-gaze-ocr"
            };

            var mockTalonDataService = new Mock<TalonVoiceCommandDataService>();
            var mockLogger = new Mock<ILogger<TalonAnalysisService>>();
            var analysisService = new TalonAnalysisService(mockTalonDataService.Object, mockLogger.Object);            // Use reflection to access private method for testing
            var method = typeof(TalonAnalysisService).GetMethod("CouldCommandsBeActiveSimultaneously", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.NotNull(method); // Ensure method was found

            // Act & Assert
            var result1vs2 = (bool)method.Invoke(analysisService, new object[] { cmd1, cmd2 })!;
            var result1vs3 = (bool)method.Invoke(analysisService, new object[] { cmd1, cmd3 })!;
            var result2vs3 = (bool)method.Invoke(analysisService, new object[] { cmd2, cmd3 })!;

            // These should all return false because the commands have different tag sets
            Assert.False(result1vs2, "Commands with different tags should not be able to be active simultaneously");
            Assert.False(result1vs3, "Commands with different tags should not be able to be active simultaneously");
            Assert.False(result2vs3, "Commands with different tags should not be able to be active simultaneously");
        }

        [Fact]
        public void CouldCommandsBeActiveSimultaneously_WithOverlappingTags_ShouldReturnTrue()
        {
            // Arrange - Commands with overlapping tags could conflict
            var cmd1 = new TalonCommand
            {
                Command = "test command",
                Tags = "user.mode1,shared_tag",
                Repository = "repo1"
            };            var cmd2 = new TalonCommand
            {
                Command = "test command",
                Tags = "user.mode2,shared_tag",
                Repository = "repo2"
            };

            // Arrange service with mocks
            var mockTalonDataService = new Mock<TalonVoiceCommandDataService>();
            var mockLogger = new Mock<ILogger<TalonAnalysisService>>();            var analysisService = new TalonAnalysisService(mockTalonDataService.Object, mockLogger.Object);

            // Use reflection to access private method for testing
            var method = typeof(TalonAnalysisService).GetMethod("CouldCommandsBeActiveSimultaneously", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.NotNull(method); // Ensure method was found

            // Act
            var result = (bool)method.Invoke(analysisService, new object[] { cmd1, cmd2 })!;

            // Assert - should return true because they share "shared_tag"
            Assert.True(result, "Commands with overlapping tags could be active simultaneously");
        }

        [Fact]
        public void CouldCommandsBeActiveSimultaneously_WithNoTags_ShouldReturnTrue()
        {
            // Arrange - Commands with no tags could potentially conflict
            var cmd1 = new TalonCommand
            {
                Command = "test command",
                Tags = "",
                Repository = "repo1"
            };            var cmd2 = new TalonCommand
            {
                Command = "test command",
                Tags = string.Empty,
                Repository = "repo2"
            };

            // Arrange service with mocks
            var mockTalonDataService = new Mock<TalonVoiceCommandDataService>();
            var mockLogger = new Mock<ILogger<TalonAnalysisService>>();
            var analysisService = new TalonAnalysisService(mockTalonDataService.Object, mockLogger.Object);            // Use reflection to access private method for testing
            var method = typeof(TalonAnalysisService).GetMethod("CouldCommandsBeActiveSimultaneously", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.NotNull(method); // Ensure method was found

            // Act
            var result = (bool)method.Invoke(analysisService, new object[] { cmd1, cmd2 })!;

            // Assert - should return true because without tags, we can't determine exclusivity
            Assert.True(result, "Commands with no tags should be considered potentially conflicting");
        }
    }
}
