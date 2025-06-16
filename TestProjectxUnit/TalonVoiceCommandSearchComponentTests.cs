using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorClassLibrary.Pages;
using Xunit;

namespace TestProjectxUnit
{
    public class TalonVoiceCommandSearchComponentTests : TestContext
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var config = new ConfigurationBuilder().Build();
            return new ApplicationDbContext(options, config);
        }

        [Fact]
        public void TalonVoiceCommandSearch_RendersWithTitleFilter()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            Services.AddSingleton(service);

            // Act
            var component = RenderComponent<TalonVoiceCommandSearch>();

            // Assert
            var titleLabel = component.Find("label.label-title");
            Assert.NotNull(titleLabel);
            Assert.Contains("Filter by Title", titleLabel.TextContent);
            
            var titleSelect = component.Find("select.filter-title");
            Assert.NotNull(titleSelect);
            Assert.Equal("i", titleSelect.GetAttribute("accesskey"));
        }

        [Fact]
        public async Task TitleFilter_PopulatesWithAvailableTitles()
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
                    Title = "File Operations", // Duplicate title
                    FilePath = "/test/file3.talon"
                }
            };
            
            await dbContext.TalonVoiceCommands.AddRangeAsync(commands);
            await dbContext.SaveChangesAsync();
            
            Services.AddSingleton(service);

            // Act
            var component = RenderComponent<TalonVoiceCommandSearch>();
            
            // Wait for component to load filter options
            await Task.Delay(100);
            
            // Assert
            var titleSelect = component.Find("select.filter-title");
            var options = titleSelect.QuerySelectorAll("option").ToList();
            
            // Should have "All Titles" plus the unique titles
            Assert.Equal(3, options.Count); // "All Titles", "Document Management", "File Operations"
            Assert.Contains(options, opt => opt.TextContent == "All Titles");
            Assert.Contains(options, opt => opt.TextContent == "Document Management");
            Assert.Contains(options, opt => opt.TextContent == "File Operations");
        }

        [Fact]
        public void TitleFilter_HasCorrectCssClasses()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            Services.AddSingleton(service);

            // Act
            var component = RenderComponent<TalonVoiceCommandSearch>();            // Assert
            var titleSelect = component.Find("select.filter-title");
            Assert.Contains("filter-title", titleSelect.GetAttribute("class")?.Split(' ') ?? Array.Empty<string>());
            Assert.Contains("form-select", titleSelect.GetAttribute("class")?.Split(' ') ?? Array.Empty<string>());
            Assert.Contains("form-select-sm", titleSelect.GetAttribute("class")?.Split(' ') ?? Array.Empty<string>());
            
            var titleLabel = component.Find("label.label-title");
            Assert.Contains("label-title", titleLabel.GetAttribute("class")?.Split(' ') ?? Array.Empty<string>());
            Assert.Contains("form-label", titleLabel.GetAttribute("class")?.Split(' ') ?? Array.Empty<string>());
            Assert.Contains("small", titleLabel.GetAttribute("class")?.Split(' ') ?? Array.Empty<string>());
        }

        [Fact]
        public void TitleFilter_HasProperAccessibility()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            Services.AddSingleton(service);

            // Act
            var component = RenderComponent<TalonVoiceCommandSearch>();

            // Assert
            var titleSelect = component.Find("select.filter-title");
            Assert.Equal("Filter by Title", titleSelect.GetAttribute("aria-label"));
            Assert.Equal("i", titleSelect.GetAttribute("accesskey"));
            
            var underlineSpan = component.Find("span.underline-title");
            Assert.NotNull(underlineSpan);
            Assert.Equal("i", underlineSpan.TextContent);
        }

        [Fact]
        public async Task TitleFilter_OnChangeUpdatesSelectedTitle()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            
            // Add a test command
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
            
            Services.AddSingleton(service);

            // Act
            var component = RenderComponent<TalonVoiceCommandSearch>();
            await Task.Delay(100); // Wait for filters to load
            
            var titleSelect = component.Find("select.filter-title");
            await titleSelect.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs 
            { 
                Value = "Test Title" 
            });

            // Assert
            Assert.Equal("Test Title", component.Instance.SelectedTitle);
        }

        [Fact]
        public async Task ClearFilters_ResetsSelectedTitle()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            Services.AddSingleton(service);

            var component = RenderComponent<TalonVoiceCommandSearch>();
            
            // Set a title filter
            component.Instance.SelectedTitle = "Some Title";
            
            // Act
            await component.Instance.ClearFilters();

            // Assert
            Assert.Equal(string.Empty, component.Instance.SelectedTitle);
        }

        [Fact]
        public async Task TitleSearch_FindsCommandsByTitle()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            
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
                    Application = "word", 
                    Mode = "command",
                    Title = "Document Management",
                    FilePath = "/test/file2.talon"
                }
            };
            
            await dbContext.TalonVoiceCommands.AddRangeAsync(commands);
            await dbContext.SaveChangesAsync();
            
            Services.AddSingleton(service);

            // Act
            var component = RenderComponent<TalonVoiceCommandSearch>();
            component.Instance.SearchTerm = "file";
            await component.InvokeAsync(() => component.Instance.OnSearch());

            // Assert
            Assert.Equal(2, component.Instance.Results.Count);
            Assert.Contains(component.Instance.Results, cmd => cmd.Command == "open file");
            Assert.Contains(component.Instance.Results, cmd => cmd.Title == "File Operations");
        }

        [Theory]
        [InlineData("File Operations")]
        [InlineData("Document Management")]
        public async Task TitleFilter_FiltersResultsBySelectedTitle(string selectedTitle)
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            
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
                    Application = "word", 
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
            
            Services.AddSingleton(service);

            // Act
            var component = RenderComponent<TalonVoiceCommandSearch>();
            component.Instance.SelectedTitle = selectedTitle;
            component.Instance.SearchTerm = ""; // No search term, only filter
            await component.InvokeAsync(() => component.Instance.OnSearch());

            // Assert
            Assert.All(component.Instance.Results, cmd => Assert.Equal(selectedTitle, cmd.Title));
            
            if (selectedTitle == "File Operations")
            {
                Assert.Equal(2, component.Instance.Results.Count);
            }
            else if (selectedTitle == "Document Management")
            {
                Assert.Single(component.Instance.Results);
            }
        }

        [Fact]
        public async Task TitleFilter_CombinesWithOtherFilters()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var service = new TalonVoiceCommandDataService(dbContext);
            
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
                    Application = "vscode", // Same app
                    Mode = "command",
                    Title = "File Operations", // Same title
                    FilePath = "/test/file2.talon"
                },
                new TalonVoiceCommand 
                { 
                    Command = "close tab", 
                    Script = "user.close_tab()", 
                    Application = "browser", // Different app
                    Mode = "command",
                    Title = "File Operations", // Same title
                    FilePath = "/test/file3.talon"
                }
            };
            
            await dbContext.TalonVoiceCommands.AddRangeAsync(commands);
            await dbContext.SaveChangesAsync();
            
            Services.AddSingleton(service);

            // Act
            var component = RenderComponent<TalonVoiceCommandSearch>();
            component.Instance.SelectedTitle = "File Operations";
            component.Instance.SelectedApplication = "vscode";
            await component.InvokeAsync(() => component.Instance.OnSearch());

            // Assert - Should only find vscode commands with "File Operations" title
            Assert.Equal(2, component.Instance.Results.Count);
            Assert.All(component.Instance.Results, cmd => 
            {
                Assert.Equal("File Operations", cmd.Title);
                Assert.Equal("vscode", cmd.Application);
            });
        }
    }
}
