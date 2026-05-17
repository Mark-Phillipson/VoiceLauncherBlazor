import { chromium } from 'playwright';

(async () => {
  let color; let matched; const browser = await chromium.launch({ headless: false });
  const page = await browser.newPage();
  const url = 'http://localhost:5008/talon-voice-command-search';
  console.log('Navigating to', url);
  try {
    await page.goto(url, { waitUntil: 'domcontentloaded' });
    await page.evaluate(() => document.documentElement.setAttribute('data-bs-theme', 'dark'));
    
    const filters = page.locator('text=Filters');
    await filters.waitFor({ state: 'visible', timeout: 5000 });
    
    // Use locator to hover instead of elementHandle to avoid detachment issues
    await filters.hover();
    await page.waitForTimeout(500);
    
    const color = await filters.evaluate((el) => getComputedStyle(el).color);

    const matched = await filters.evaluate((el) => {
        const rules = [];
        for (const sheet of Array.from(document.styleSheets)) {
            let cssRules;
            try { cssRules = sheet.cssRules; } catch (e) { continue; }
            if (!cssRules) continue;
            for (const r of Array.from(cssRules)) {
                if (r.type !== 1) continue; // CSSRule.STYLE_RULE
                const sr = r;
                const sel = sr.selectorText;
                if (!sel) continue;
                if (sel.includes(':')) continue;
                try {
                    if (el.matches(sel)) {
                        const colorVal = sr.style.getPropertyValue('color');
                        if (colorVal) rules.push({ selector: sel, color: colorVal });
                    }
                } catch (e) {}
            }
        }
        return rules;
    });

    console.log('Computed color:', color);
    console.log('Matched color rules:', JSON.stringify(matched, null, 2));
  } catch (err) {
    console.error('Error during execution:', err);
  } finally {
      // persist results for inspection
      try {
        const fs = await import('fs');
        const out = { color, matched };
        fs.writeFileSync('PlaywrightTests/test-results/debug-css.json', JSON.stringify(out, null, 2));
      } catch (e) {
        console.error('Failed to write debug output', e);
      }

      await browser.close();
  }
})();

