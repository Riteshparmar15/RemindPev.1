// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Counter Privacy Shield (Removed Verification)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('Privacy button is removed and financial figures display unmasked directly', async ({ page }) => {
    // Privacy toggle button is removed
    const privacyBtn = page.locator('#btnPrivacyToggle');
    await expect(privacyBtn).toHaveCount(0);

    // Financial figures are displayed directly without mask
    const statReceivable = page.locator('#statReceivable');
    await expect(statReceivable).toBeVisible();
    const text = await statReceivable.innerText();
    expect(text).not.toContain('••••••');
  });
});
