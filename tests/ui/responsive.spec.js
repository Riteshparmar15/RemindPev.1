// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Responsive & Mobile Viewport Experience', () => {
  test('Mobile viewport (375x667) renders without horizontal scroll', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Check document scrollWidth vs innerWidth
    const hasHorizontalOverflow = await page.evaluate(() => {
      return document.documentElement.scrollWidth > window.innerWidth;
    });
    expect(hasHorizontalOverflow).toBe(false);

    // Verify topbar branding is visible and clean
    const logo = page.locator('#topbarBrandLogo');
    await expect(logo).toBeVisible();
  });

  test('Mobile bottom navigation bar is visible and navigates views', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const bottomNav = page.locator('#mobileBottomNav');
    await expect(bottomNav).toBeVisible();

    // Click Customers tab
    const custTab = page.locator('#mobileTabCustomers');
    await custTab.click();
    await expect(page.locator('#customersModule')).toBeVisible();

    // Click Overview tab
    const overviewTab = page.locator('#mobileTabOverview');
    await overviewTab.click();
    await expect(page.locator('#overviewModule')).toBeVisible();
  });

  test('Tablet viewport (768x1024) adapts layout cleanly', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const hasHorizontalOverflow = await page.evaluate(() => {
      return document.documentElement.scrollWidth > window.innerWidth;
    });
    expect(hasHorizontalOverflow).toBe(false);
  });
});
