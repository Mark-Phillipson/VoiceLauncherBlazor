using System.Collections.Generic;
using DataAccessLibrary.DTO;
using RazorClassLibrary.Pages;
using Xunit;

namespace TestProjectxUnit;

public class CustomIntelliSenseTableFilterTests
{
    [Fact]
    public void MatchesLiveTextFilter_ShouldKeepSearchFiltersSeparateFromMetadataFilters()
    {
        var item = new CustomIntelliSenseDTO
        {
            DisplayValue = "Open File",
            SendKeysValue = "user.open_file()",
            LanguageName = "C Sharp",
            CategoryName = "Window",
            CommandType = "Snippet",
            DeliveryType = "Keyboard"
        };

        Assert.True(CustomIntelliSenseTable.MatchesSearchTextFilter(item, "file"));
        Assert.True(CustomIntelliSenseTable.MatchesLanguageFilter(item, "sharp"));
        Assert.True(CustomIntelliSenseTable.MatchesCategoryFilter(item, "win"));
        Assert.False(CustomIntelliSenseTable.MatchesLanguageFilter(item, "notfound"));
        Assert.False(CustomIntelliSenseTable.MatchesSearchTextFilter(item, "notfound"));
    }

    [Fact]
    public void MatchesLanguageAndCategoryFilters_ShouldAllowMultipleTextFragments()
    {
        var item = new CustomIntelliSenseDTO
        {
            LanguageName = "C Sharp Python",
            CategoryName = "Window File"
        };

        Assert.True(CustomIntelliSenseTable.MatchesLanguageFilter(item, "sharp python"));
        Assert.True(CustomIntelliSenseTable.MatchesCategoryFilter(item, "window,file"));
        Assert.True(CustomIntelliSenseTable.MatchesCategoryFilter(item, "file window"));
    }

    [Fact]
    public void ResolveSelectedIds_ShouldResolveCategoryAndLanguageIdsFromTextValues()
    {
        var languages = new List<LanguageDTO>
        {
            new() { Id = 8, LanguageName = "C Sharp", Colour = "#000000" },
            new() { Id = 9, LanguageName = "Python", Colour = "#111111" }
        };

        var categories = new List<CategoryDTO>
        {
            new() { Id = 34, CategoryName = "Window", Colour = "#000000" },
            new() { Id = 35, CategoryName = "File", Colour = "#111111" }
        };

        var result = CustomIntelliSenseTable.ResolveSelectedIds(languages, categories, "C Sharp", "Window");

        Assert.Equal(8, result.LanguageId);
        Assert.Equal(34, result.CategoryId);
    }
}
