// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Dynamic UPI QR Payments (Removed Verification)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('Store QR buttons and UPI QR modal are removed from application', async ({ page }) => {
    // Store QR button in header is removed
    const openQRBtn = page.locator('button:has-text("Store QR"), button[onclick*="openUPIQRModal"]');
    await expect(openQRBtn).toHaveCount(0);

    // UPI QR modal is removed
    const modal = page.locator('#upiQRModal');
    await expect(modal).toHaveCount(0);
  });
});
