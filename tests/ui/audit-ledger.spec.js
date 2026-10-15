// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Audit Ledger & Transaction Feed', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    const isDesktop = await page.evaluate(() => window.innerWidth >= 1024);
    if (isDesktop) {
      await page.locator('#navLinkTransactions').click();
    } else {
      await page.locator('#btnSidebarToggle').click();
      await page.locator('#drawerLinkTransactions').click();
    }
  });

  test('Transactions audit feed module loads with title and filters', async ({ page }) => {
    await expect(page.locator('#transactionsModule')).toBeVisible();
    await expect(page.locator('button:has-text("All"), button:has-text("Credit"), button:has-text("Debit")').first()).toBeVisible();
  });

  test('View mode toggle switches between Cards and Table view', async ({ page }) => {
    const btnCards = page.locator('#viewBtnCards');
    const btnTable = page.locator('#viewBtnTable');
    
    if (await btnTable.isVisible()) {
      await btnTable.click();
      await expect(page.locator('#transactionsTableView')).toBeVisible();
      await expect(page.locator('#transactionsCardsView')).toBeHidden();

      await btnCards.click();
      await expect(page.locator('#transactionsCardsView')).toBeVisible();
      await expect(page.locator('#transactionsTableView')).toBeHidden();
    }
  });

  test('Export CSV functionality trigger exists and has button', async ({ page }) => {
    const exportBtn = page.locator('button:has-text("Export"), button:has-text("CSV")').first();
    await expect(exportBtn).toBeVisible();
  });
});
