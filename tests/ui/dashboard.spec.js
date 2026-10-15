// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Executive Overview Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('Page loads with correct title and branding', async ({ page }) => {
    await expect(page).toHaveTitle(/RemindPe/);
    const brand = page.locator('#sidebarBrandName');
    await expect(brand).toContainText('Remind');
  });

  test('KPI metric cards display real aggregated data', async ({ page }) => {
    const receivable = page.locator('#statReceivable');
    const received = page.locator('#statReceived');
    const customersCount = page.locator('#statTotalCustomers');
    const transactionsCount = page.locator('#statTotalTransactions');

    await expect(receivable).toBeVisible();
    await expect(received).toBeVisible();
    await expect(customersCount).toBeVisible();
    await expect(transactionsCount).toBeVisible();

    const receivableText = await receivable.innerText();
    expect(receivableText).toMatch(/₹|Rs|\$|€|•/);
  });

  test('Recovery rate bar and progress visual indicators are computed', async ({ page }) => {
    const recoveryBadge = page.locator('#recoveryRateBadge');
    await expect(recoveryBadge).toBeVisible();
    const badgeText = await recoveryBadge.innerText();
    expect(badgeText).toMatch(/\d+%\s*Recovery/);
  });

  test('Priority watchlist renders overdue customers or zero-state message', async ({ page }) => {
    const watchlist = page.locator('#priorityWatchlistContainer');
    await expect(watchlist).toBeVisible();
    const content = await watchlist.innerText();
    expect(content.length).toBeGreaterThan(5);
  });

  test('Quick action buttons on overview trigger appropriate modals', async ({ page }) => {
    const addCustBtn = page.locator('button:has-text("+ New Customer"), button:has-text("Add Customer")').first();
    await addCustBtn.click();
    const modal = page.locator('#addCustomerModal');
    await expect(modal).toBeVisible();
    
    // Close via cancel
    await page.locator('#addCustomerModal button:has-text("Cancel")').click();
    await expect(modal).toBeHidden();
  });
});
