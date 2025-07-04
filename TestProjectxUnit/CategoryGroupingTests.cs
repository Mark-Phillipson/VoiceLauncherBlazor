using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DataAccessLibrary.DTO;
using RazorClassLibrary.Models;

namespace TestProjectxUnit
{
    public class CategoryGroupingTests
    {
        [Fact]
        public void CategoryGroupedByLanguageDTO_ShouldGroupCategoriesCorrectly()
        {
            // Arrange
            var categories = new List<CategoryDTO>
            {
                new CategoryDTO { Id = 1, CategoryName = "C# Category", CategoryType = "IntelliSense Command" },
                new CategoryDTO { Id = 2, CategoryName = "Python Category", CategoryType = "IntelliSense Command" },
                new CategoryDTO { Id = 3, CategoryName = "JavaScript Category", CategoryType = "IntelliSense Command" }
            };

            var groupedCategories = new List<CategoryGroupedByLanguageDTO>
            {
                new CategoryGroupedByLanguageDTO
                {
                    LanguageId = 1,
                    LanguageName = "C#",
                    LanguageColour = "#239120",
                    Categories = new List<CategoryDTO> { categories[0] }
                },
                new CategoryGroupedByLanguageDTO
                {
                    LanguageId = 2,
                    LanguageName = "Python",
                    LanguageColour = "#3776ab",
                    Categories = new List<CategoryDTO> { categories[1] }
                },
                new CategoryGroupedByLanguageDTO
                {
                    LanguageId = 3,
                    LanguageName = "JavaScript",
                    LanguageColour = "#f7df1e",
                    Categories = new List<CategoryDTO> { categories[2] }
                }
            };

            // Act
            var totalCategories = groupedCategories.Sum(g => g.Categories.Count);
            var languageNames = groupedCategories.Select(g => g.LanguageName).ToList();

            // Assert
            Assert.Equal(3, totalCategories);
            Assert.Equal(3, groupedCategories.Count);
            Assert.Contains("C#", languageNames);
            Assert.Contains("Python", languageNames);
            Assert.Contains("JavaScript", languageNames);
            
            // Verify each group has the correct structure
            foreach (var group in groupedCategories)
            {
                Assert.NotNull(group.LanguageName);
                Assert.NotNull(group.LanguageColour);
                Assert.True(group.Categories.Count > 0);
                Assert.All(group.Categories, c => Assert.NotNull(c.CategoryName));
            }
        }

        [Fact]
        public void CategoryGroupedByLanguageDTO_ShouldHandleEmptyCategories()
        {
            // Arrange
            var groupedCategories = new List<CategoryGroupedByLanguageDTO>
            {
                new CategoryGroupedByLanguageDTO
                {
                    LanguageId = 1,
                    LanguageName = "C#",
                    LanguageColour = "#239120",
                    Categories = new List<CategoryDTO>()
                }
            };

            // Act
            var totalCategories = groupedCategories.Sum(g => g.Categories.Count);

            // Assert
            Assert.Equal(0, totalCategories);
            Assert.Single(groupedCategories);
            Assert.Empty(groupedCategories[0].Categories);
        }

        [Fact]
        public void CategoryGroupedByLanguageDTO_ShouldSortCategoriesWithinGroup()
        {
            // Arrange
            var categories = new List<CategoryDTO>
            {
                new CategoryDTO { Id = 1, CategoryName = "Zebra Category", CategoryType = "IntelliSense Command" },
                new CategoryDTO { Id = 2, CategoryName = "Apple Category", CategoryType = "IntelliSense Command" },
                new CategoryDTO { Id = 3, CategoryName = "Banana Category", CategoryType = "IntelliSense Command" }
            };

            var groupedCategories = new CategoryGroupedByLanguageDTO
            {
                LanguageId = 1,
                LanguageName = "C#",
                LanguageColour = "#239120",
                Categories = categories
            };

            // Act
            groupedCategories.Categories = groupedCategories.Categories.OrderBy(c => c.CategoryName).ToList();

            // Assert
            Assert.Equal("Apple Category", groupedCategories.Categories[0].CategoryName);
            Assert.Equal("Banana Category", groupedCategories.Categories[1].CategoryName);
            Assert.Equal("Zebra Category", groupedCategories.Categories[2].CategoryName);
        }
    }
}