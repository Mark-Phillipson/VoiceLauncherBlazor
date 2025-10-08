using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DataAccessLibrary.DTO;

namespace TestProjectxUnit
{
	public class VSCodeProjectsLauncherTests
	{
		[Fact]
		public void VSCodeProjectCategory_ShouldBeDetectedCaseInsensitive()
		{
			// Arrange
			var categories = new List<CategoryDTO>
			{
				new CategoryDTO { Id = 1, CategoryName = "VS Code", CategoryType = "VS Code Projects" },
				new CategoryDTO { Id = 2, CategoryName = "My Projects", CategoryType = "vs code projects" },
				new CategoryDTO { Id = 3, CategoryName = "Folders", CategoryType = "Launch Applications" },
				new CategoryDTO { Id = 4, CategoryName = "Apps", CategoryType = "Launch Applications" }
			};

			var launcher1 = new LauncherDTO { Id = 1, CategoryId = 1, CommandLine = @"C:\Projects\VoiceLauncher" };
			var launcher2 = new LauncherDTO { Id = 2, CategoryId = 2, CommandLine = @"C:\Projects\AnotherProject" };
			var launcher3 = new LauncherDTO { Id = 3, CategoryId = 3, CommandLine = @"C:\Users\Documents" };

			// Act
			var category1 = categories.FirstOrDefault(c => c.Id == launcher1.CategoryId);
			var isVSCode1 = category1?.CategoryType?.Equals("VS Code Projects", StringComparison.OrdinalIgnoreCase) == true;

			var category2 = categories.FirstOrDefault(c => c.Id == launcher2.CategoryId);
			var isVSCode2 = category2?.CategoryType?.Equals("VS Code Projects", StringComparison.OrdinalIgnoreCase) == true;

			var category3 = categories.FirstOrDefault(c => c.Id == launcher3.CategoryId);
			var isVSCode3 = category3?.CategoryType?.Equals("VS Code Projects", StringComparison.OrdinalIgnoreCase) == true;

			// Assert
			Assert.True(isVSCode1, "Category with type 'VS Code Projects' should be detected");
			Assert.True(isVSCode2, "Category with type 'vs code projects' (lowercase) should be detected");
			Assert.False(isVSCode3, "Category with type 'Launch Applications' should not be detected as VS Code Project");
		}

		[Fact]
		public void VSCodeProjectCategory_ShouldHandleNullCategory()
		{
			// Arrange
			var categories = new List<CategoryDTO>
			{
				new CategoryDTO { Id = 1, CategoryName = "Test", CategoryType = "VS Code Projects" }
			};

			var launcher = new LauncherDTO { Id = 1, CategoryId = 999, CommandLine = @"C:\Projects\Test" };

			// Act
			var category = categories.FirstOrDefault(c => c.Id == launcher.CategoryId);
			var isVSCode = category?.CategoryType?.Equals("VS Code Projects", StringComparison.OrdinalIgnoreCase) == true;

			// Assert
			Assert.False(isVSCode, "Launcher with non-existent category should not be detected as VS Code Project");
		}

		[Fact]
		public void VSCodeProjectCategory_ShouldHandleNullCategoryType()
		{
			// Arrange
			var categories = new List<CategoryDTO>
			{
				new CategoryDTO { Id = 1, CategoryName = "Test", CategoryType = null }
			};

			var launcher = new LauncherDTO { Id = 1, CategoryId = 1, CommandLine = @"C:\Projects\Test" };

			// Act
			var category = categories.FirstOrDefault(c => c.Id == launcher.CategoryId);
			var isVSCode = category?.CategoryType?.Equals("VS Code Projects", StringComparison.OrdinalIgnoreCase) == true;

			// Assert
			Assert.False(isVSCode, "Category with null CategoryType should not be detected as VS Code Project");
		}

		[Fact]
		public void VSCodeProjectLauncher_ShouldHaveValidCommandLinePath()
		{
			// Arrange
			var category = new CategoryDTO { Id = 1, CategoryName = "VS Code", CategoryType = "VS Code Projects" };
			var launcher = new LauncherDTO 
			{ 
				Id = 1, 
				CategoryId = 1, 
				Name = "Voice Launcher",
				CommandLine = @"C:\Projects\VoiceLauncherBlazor" 
			};

			// Act
			var isValidPath = !string.IsNullOrWhiteSpace(launcher.CommandLine);
			var isVSCodeProject = category.CategoryType?.Equals("VS Code Projects", StringComparison.OrdinalIgnoreCase) == true;

			// Assert
			Assert.True(isValidPath, "VS Code Project launcher should have a valid CommandLine path");
			Assert.True(isVSCodeProject, "Launcher should belong to VS Code Projects category");
			Assert.Equal("Voice Launcher", launcher.Name);
		}
	}
}
