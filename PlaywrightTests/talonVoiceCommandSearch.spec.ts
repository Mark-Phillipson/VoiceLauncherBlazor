// Playwright test for Talon Voice Command Search
import { test, expect } from '@playwright/test';

test.describe('Talon Voice Command Search', () => {
  test('should search for "game" and display results', async ({ page }) => {
    // Run in headed mode for visual inspection
    await page.goto('http://localhost:7264'); // Adjust port if needed

    // Navigate to Talon Voice Command Search page/component
    // If there is a direct route, update the selector below
    await page.waitForSelector('input[placeholder*="Search names"]');

    // Type "game" into the search names textbox
    await page.fill('input[placeholder*="Search names"]', 'game');

    // Click the search button (update selector if needed)
    await page.click('button:has-text("Search")');

    // Wait for results to appear
    await page.waitForSelector('.search-results-container');

    // Assert that results are shown (update selector for result items)
    const results = await page.$$('.search-results-container .result-item');
    expect(results.length).toBeGreaterThan(0);
  });
});
