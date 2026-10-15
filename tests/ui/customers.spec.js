// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Customer Accounts & Digital Ledger', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    // Switch to customers module
    await page.locator('#navLinkCustomers:visible, #mobileTabCustomers:visible, #drawerLinkCustomers:visible').first().click();
  });

  test('Customers view loads with count badge and search input', async ({ page }) => {
    await expect(page.locator('#customersModule')).toBeVisible();
    await expect(page.locator('#searchInput')).toBeVisible();
    const countBadge = page.locator('#customerCountBadge');
    await expect(countBadge).toBeVisible();
  });

  test('Customer search filters list dynamically', async ({ page }) => {
    const searchInput = page.locator('#searchInput');
    await searchInput.fill('NonExistentCustomerXYZ999');
    await page.waitForTimeout(350);

    const emptyText = page.locator('#customersList');
    await expect(emptyText).toContainText('No accounts match criteria');

    // Clear search
    await page.locator('#clearSearchBtn').click();
    await page.waitForTimeout(350);
    await expect(emptyText).not.toContainText('No accounts match criteria');
  });

  test('Filter tabs (All, Pending, Settled, Advance) work without error', async ({ page }) => {
    // Click Pending filter
    await page.locator('button:has-text("Pending Dues")').first().click();
    await page.waitForTimeout(100);
    await expect(page.locator('#customersList')).toBeVisible();

    // Click Settled filter
    await page.locator('button:has-text("Settled")').first().click();
    await page.waitForTimeout(100);
    await expect(page.locator('#customersList')).toBeVisible();

    // Reset to All
    await page.locator('button:has-text("All Accounts")').first().click();
    await page.waitForTimeout(100);
    await expect(page.locator('#customersList')).toBeVisible();
  });

  test('Add customer modal validates input correctly', async ({ page }) => {
    await page.locator('button:has-text("+ Add Customer"), button:has-text("Add Customer")').first().click();
    const modal = page.locator('#addCustomerModal');
    await expect(modal).toBeVisible();

    // Submitting empty should trigger required validation
    const submitBtn = modal.locator('button[type="submit"]');
    await submitBtn.click();

    // Name field is required
    const nameInput = page.locator('#custName');
    const isInvalid = await nameInput.evaluate((el) => !el.checkValidity());
    expect(isInvalid).toBe(true);

    // Close modal
    await modal.locator('button:has-text("Cancel")').click();
    await expect(modal).toBeHidden();
  });
});
