using RazorClassLibrary.Services;
using RazorClassLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Reflection;

namespace TestProjectxUnit
{
    public class TalonTagConflictTestsNew
    {
        [Fact]
        public void ParseTags_WithCommaSeparatedTags_ShouldReturnCorrectList()
        {
            // We'll test the ParseTags method using reflection since it's private
            // This simulates what we see in your screenshot
            var tagString = "user.command_wizard_showing,talon_ui_helper";
            
            // Create a simple logger for testing
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<TalonAnalysisService>();
            
            // Create a mock data service - we'll pass null since we're only testing tag parsing
            var analysisService = new TalonAnalysisService(null!, logger);
            
            // Use reflection to test private method
            var method = typeof(TalonAnalysisService).GetMethod("ParseTags", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            Assert.NotNull(method);
            
            // Act
            var result = method.Invoke(analysisService, new object[] { tagString }) as List<string>;
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("user.command_wizard_showing", result);
            Assert.Contains("talon_ui_helper", result);
        }

        [Fact]
        public void ParseTags_WithSingleTag_ShouldReturnSingleItem()
        {
            // Test with single tag like "community"
            var tagString = "community";
            
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<TalonAnalysisService>();
            var analysisService = new TalonAnalysisService(null!, logger);
            
            var method = typeof(TalonAnalysisService).GetMethod("ParseTags", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            Assert.NotNull(method);
            
            var result = method.Invoke(analysisService, new object[] { tagString }) as List<string>;
            
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains("community", result);
        }

        [Fact]
        public void ParseTags_WithEmptyString_ShouldReturnEmptyList()
        {
            // Test with empty or null tags
            var tagString = "";
            
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<TalonAnalysisService>();
            var analysisService = new TalonAnalysisService(null!, logger);
            
            var method = typeof(TalonAnalysisService).GetMethod("ParseTags", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            Assert.NotNull(method);
            
            var result = method.Invoke(analysisService, new object[] { tagString }) as List<string>;
            
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CouldCommandsBeActiveSimultaneously_WithDifferentTags_ShouldReturnFalse()
        {
            // Test the logic that should prevent the "choose <number_small>" commands from being flagged as conflicts
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<TalonAnalysisService>();
            var analysisService = new TalonAnalysisService(null!, logger);
            
            // Create commands with different tag sets (like in your screenshot)
            var cmd1 = new TalonCommand { Tags = "community" };
            var cmd2 = new TalonCommand { Tags = "user.command_wizard_showing,talon_ui_helper" };
            
            var method = typeof(TalonAnalysisService).GetMethod("CouldCommandsBeActiveSimultaneously", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            Assert.NotNull(method);
            
            // Act
            var result = (bool)method.Invoke(analysisService, new object[] { cmd1, cmd2 })!;
            
            // Assert - these should NOT be able to be active simultaneously (no shared tags)
            Assert.False(result, "Commands with completely different tags should not be flagged as conflicts");
        }
    }
}
