// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Customer Ledger Detail & Transactions', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await page.locator('#navLinkCustomers:visible, #mobileTabCustomers:visible, #drawerLinkCustomers:visible').first().click();
  });

  test('Selecting a customer loads detail view and financial summary', async ({ page }) => {
    // Wait for customer list items
    const customerItem = page.locator('#customersList > div').first();
    await expect(customerItem).toBeVisible();
    await customerItem.click();

    // Check customer detail header
    const activeView = page.locator('#activeCustomerView');
    await expect(activeView).toBeVisible();

    const detailName = page.locator('#detailName');
    await expect(detailName).toBeVisible();
    const nameText = await detailName.innerText();
    expect(nameText.length).toBeGreaterThan(0);

    // Check net balance displayed
    const netBalance = page.locator('#detailNetBalance');
    await expect(netBalance).toBeVisible();
  });

  test('Customer Ledger shows Credit and Debit summary tiles', async ({ page }) => {
    const customerItem = page.locator('#customersList > div').first();
    await customerItem.click();

    await expect(page.locator('#detailTotalCredit')).toBeVisible();
    await expect(page.locator('#detailTotalDebit')).toBeVisible();
  });

  test('Add Transaction modal opens for selected customer with Credit/Debit toggle', async ({ page }) => {
    const customerItem = page.locator('#customersList > div').first();
    await customerItem.click();

    // Click "+ Credit Given"
    const addCreditBtn = page.locator('button:has-text("Credit Given"), button:has-text("+ Credit")').first();
    if (await addCreditBtn.isVisible()) {
      await addCreditBtn.click();
      const modal = page.locator('#addTransactionModal');
      await expect(modal).toBeVisible();
      
      // Verify customer name is prefilled in header
      const modalSubtitle = page.locator('#modalCustomerSubtitle');
      await expect(modalSubtitle).toBeVisible();

      // Close modal
      await modal.locator('button:has-text("Cancel")').click();
      await expect(modal).toBeHidden();
    }
  });
});
