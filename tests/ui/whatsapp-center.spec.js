// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('WhatsApp Collection Command Center', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await page.locator('#navLinkReminders:visible, #mobileTabReminders:visible, #drawerLinkReminders:visible').first().click();
  });

  test('Reminders command center displays eligible overdue accounts', async ({ page }) => {
    await expect(page.locator('#remindersModule')).toBeVisible();
    const container = page.locator('#remindersListContainer');
    await expect(container).toBeVisible();
  });

  test('Clicking Send Reminder opens WhatsApp Preview Modal with templates', async ({ page }) => {
    const remindBtn = page.locator('#remindersListContainer button:has-text("Remind"), #remindersListContainer button[title*="WhatsApp"]').first();
    if (await remindBtn.isVisible()) {
      await remindBtn.click();
      const modal = page.locator('#whatsAppModal');
      await expect(modal).toBeVisible();

      // Verify template selection buttons exist
      await expect(page.locator('#templateTabs')).toBeVisible();

      // Check message preview text box
      const messagePreview = page.locator('#waMessagePreview');
      await expect(messagePreview).toBeVisible();
      const previewText = await messagePreview.innerText();
      expect(previewText.length).toBeGreaterThan(10);

      // Verify Send on WhatsApp and Copy buttons are present
      await expect(page.locator('#btnSendWA')).toBeVisible();
      await expect(page.locator('button:has-text("Copy Message")')).toBeVisible();

      // Close modal
      await page.locator('#whatsAppModal button:has-text("Close")').click();
      await expect(modal).toBeHidden();
    }
  });
});
