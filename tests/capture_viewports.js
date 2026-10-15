const { chromium } = require('@playwright/test');
const path = require('path');
const fs = require('fs');

const viewports = [
  { name: 'small-mobile_320x568', width: 320, height: 568 },
  { name: 'standard-mobile_375x812', width: 375, height: 812 },
  { name: 'large-mobile_430x932', width: 430, height: 932 },
  { name: 'tablet_768x1024', width: 768, height: 1024 },
  { name: 'laptop_1366x768', width: 1366, height: 768 },
  { name: 'desktop_1440x900', width: 1440, height: 900 },
  { name: 'large-desktop_1920x1080', width: 1920, height: 1080 }
];

async function capture() {
  const browser = await chromium.launch({ headless: true });
  const outDir = path.join(__dirname, 'screenshots');
  if (!fs.existsSync(outDir)) {
    fs.mkdirSync(outDir, { recursive: true });
  }

  const results = [];

  for (const vp of viewports) {
    const page = await browser.newPage({
      viewport: { width: vp.width, height: vp.height },
      deviceScaleFactor: 1
    });

    await page.goto('http://localhost:5033', { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);

    const hasHorizontalOverflow = await page.evaluate(() => {
      return document.documentElement.scrollWidth > window.innerWidth;
    });

    const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
    const windowWidth = await page.evaluate(() => window.innerWidth);

    const screenshotPath = path.join(outDir, `${vp.name}.png`);
    await page.screenshot({ path: screenshotPath, fullPage: false });

    results.push({
      viewport: vp.name,
      dimensions: `${vp.width}x${vp.height}`,
      hasOverflow: hasHorizontalOverflow,
      scrollWidth,
      windowWidth,
      screenshot: screenshotPath
    });

    console.log(`[Viewport: ${vp.name}] - Overflow: ${hasHorizontalOverflow} (scrollWidth: ${scrollWidth}, innerWidth: ${windowWidth})`);
    await page.close();
  }

  await browser.close();
  fs.writeFileSync(path.join(outDir, 'viewport_results.json'), JSON.stringify(results, null, 2));
  console.log('Viewport validation complete!');
}

capture().catch(err => {
  console.error(err);
  process.exit(1);
});
