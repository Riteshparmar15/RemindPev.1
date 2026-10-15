// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Navigation, Routing & Shared UI', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('Sidebar toggle button toggles desktop sidebar visibility', async ({ page }) => {
    const toggleBtn = page.locator('#btnSidebarToggle');
    await expect(toggleBtn).toBeVisible();

    const isDesktop = await page.evaluate(() => window.innerWidth >= 1024);
    if (isDesktop) {
      const sidebar = page.locator('#appSidebar');
      const isInitiallyVisible = await sidebar.isVisible();

      // Click toggle
      await toggleBtn.click();
      await page.waitForTimeout(100);

      const isAfterClick = await sidebar.isVisible();
      expect(isAfterClick).not.toBe(isInitiallyVisible);

      // Reset
      await toggleBtn.click();
      await page.waitForTimeout(100);
    } else {
      const drawer = page.locator('#mobileDrawer');
      await expect(drawer).toBeHidden();
      await toggleBtn.click();
      await expect(drawer).toBeVisible();
      // Close drawer
      await page.locator('#mobileDrawer button:has-text(""), #mobileDrawer button[onclick*="toggleMobileDrawer"]').first().click();
      await expect(drawer).toBeHidden();
    }
  });

  test('Brand logo navigates back to Overview from sub-modules', async ({ page }) => {
    // Navigate to customers
    await page.locator('#navLinkCustomers:visible, #mobileTabCustomers:visible, #drawerLinkCustomers:visible').first().click();
    await expect(page.locator('#customersModule')).toBeVisible();

    // Click Brand logo
    await page.locator('#topbarBrandLogo').click();
    await expect(page.locator('#overviewModule')).toBeVisible();
  });

  test('Keyboard shortcut (Escape) closes open modals', async ({ page }) => {
    // Open settings modal
    await page.locator('#topbarSettingsBtn, button:has-text("Settings"):visible, button[onclick*="openSettingsModal"]:visible').first().click();
    const settingsModal = page.locator('#settingsModal');
    await expect(settingsModal).toBeVisible();

    // Press Escape
    await page.keyboard.press('Escape');
    await expect(settingsModal).toBeHidden();
  });
});
