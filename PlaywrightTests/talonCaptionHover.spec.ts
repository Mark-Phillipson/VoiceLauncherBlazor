// Playwright visual test for caption hover color in dark theme
import { test, expect } from '@playwright/test';

test.describe('Talon caption hover (visual)', () => {
  test('Filters caption remains readable on hover in dark theme', async ({ page }) => {
    const url = 'http://localhost:5008/talon-voice-command-search';
    // Use DOMContentLoaded to avoid blocking on external embeds (networkidle can hang)
    await page.goto(url, { waitUntil: 'domcontentloaded' });

    // Force dark theme for deterministic rendering
    await page.evaluate(() => document.documentElement.setAttribute('data-bs-theme', 'dark'));

    // Wait for header to appear
    const filters = page.locator('text=Filters');
    await expect(filters).toBeVisible({ timeout: 5000 });

    // Hover with retries in case the element becomes detached during DOM updates
    let color: string | null = null;
    for (let attempt = 0; attempt < 4; attempt++) {
      try {
        await filters.hover();
        // allow CSS hover effect to apply
        await page.waitForTimeout(200);

        const handle = await filters.elementHandle();
        if (!handle) throw new Error('Filters element not found for hover assertion');

        color = await page.evaluate((el) => getComputedStyle(el).color, handle);
        // Debug: find stylesheet rules that set `color` and match this element
        const matched = await page.evaluate((el) => {
          const rules = [] as any[];
          for (const sheet of Array.from(document.styleSheets)) {
            let cssRules;
            try { cssRules = sheet.cssRules; } catch (e) { continue; }
            for (const r of Array.from(cssRules)) {
              if (r.type !== CSSRule.STYLE_RULE) continue;
              const sel = (r as CSSStyleRule).selectorText;
              if (!sel) continue;
              // skip complex pseudo selectors to avoid exceptions
              if (sel.includes(':')) continue;
              try {
                if ((el as Element).matches(sel)) {
                  const colorVal = (r as CSSStyleRule).style.getPropertyValue('color');
                  if (colorVal) rules.push({ selector: sel, color: colorVal });
                }
              } catch (e) {
                // ignore selectors that throw
              }
            }
          }
          return rules;
        }, handle);
        if (matched && matched.length) console.log('matchedColorRules:', JSON.stringify(matched));
        break;
      } catch (err) {
        if (attempt === 3) throw err;
        // brief backoff before retrying
        await page.waitForTimeout(150);
      }
    }

    if (!color) throw new Error('Unable to determine color after hover attempts');

    // Parse rgb(...) and compute perceived brightness
    const nums = color.match(/\d+/g)?.map((s) => Number(s)) ?? [0, 0, 0];
    const brightness = (nums[0] * 299 + nums[1] * 587 + nums[2] * 114) / 1000;

    // Save screenshot for reviewer/debugging
    await page.screenshot({ path: 'PlaywrightTests/test-results/filters-hover-dark.png', fullPage: false });

    // Assert brightness is not near-black (threshold chosen conservatively)
    expect(brightness).toBeGreaterThan(100);
  });
});
