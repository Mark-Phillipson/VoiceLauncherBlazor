using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class DetailsKeyboardNavigationTests : PageTest
{
    private static readonly string BaseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASEURL") ?? "http://localhost:5000";

    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync(BaseUrl + "/test-details.html", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
        try { await Page.SetViewportSizeAsync(1280, 900); } catch { }
    }

    [Test]
    public async Task ArrowDown_Selects_First_Item()
    {
        var summary = Page.Locator("#test-summary");
        await Expect(summary).ToBeVisibleAsync();
        await summary.ClickAsync();

        // Press ArrowDown -> first item
        await Page.Keyboard.PressAsync("ArrowDown");
        var isFocused1 = await Page.Locator("#item1").EvaluateAsync<bool>("el => el === document.activeElement");
        Assert.IsTrue(isFocused1, "item1 should be focused after ArrowDown from summary");

        // Press ArrowDown -> second item
        await Page.Keyboard.PressAsync("ArrowDown");
        var isFocused2 = await Page.Locator("#item2").EvaluateAsync<bool>("el => el === document.activeElement");
        Assert.IsTrue(isFocused2, "item2 should be focused after second ArrowDown");

        // ArrowUp returns to first
        await Page.Keyboard.PressAsync("ArrowUp");
        var isFocused3 = await Page.Locator("#item1").EvaluateAsync<bool>("el => el === document.activeElement");
        Assert.IsTrue(isFocused3, "item1 should be focused after ArrowUp");
    }

    [Test]
    public async Task Escape_Closes_Details_And_Returns_Focus()
    {
        var summary = Page.Locator("#test-summary");
        await Expect(summary).ToBeVisibleAsync();
        await summary.ClickAsync();

        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("Escape");

        var isOpen = await Page.EvaluateAsync<bool>("() => document.querySelector('#test-details').hasAttribute('open')");
        Assert.IsFalse(isOpen, "Details should be closed after Escape");

        var isSummaryFocused = await Page.EvaluateAsync<bool>("() => document.activeElement && document.activeElement.id === 'test-summary'");
        Assert.IsTrue(isSummaryFocused, "Summary should have focus after Escape");
    }
}
