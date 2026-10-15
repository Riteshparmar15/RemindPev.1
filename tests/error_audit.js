const { chromium } = require('@playwright/test');

async function checkConsoleAndNetwork() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();

  const consoleErrors = [];
  const pageErrors = [];
  const networkFailures = [];

  page.on('console', msg => {
    if (msg.type() === 'error') {
      consoleErrors.push(msg.text());
    }
  });

  page.on('pageerror', err => {
    pageErrors.push(err.message);
  });

  page.on('requestfailed', request => {
    networkFailures.push(`${request.method()} ${request.url()} - ${request.failure()?.errorText}`);
  });

  console.log('1. Loading Overview page...');
  await page.goto('http://localhost:5033', { waitUntil: 'networkidle' });

  console.log('2. Navigating to Customers Module...');
  await page.locator('#navLinkCustomers, #mobileTabCustomers').first().click();
  await page.waitForTimeout(500);

  console.log('3. Selecting a Customer...');
  const customerItem = page.locator('#customersList .customer-item').first();
  if (await customerItem.count() > 0) {
    await customerItem.click();
    await page.waitForTimeout(500);
  }

  console.log('4. Navigating to Transactions Module...');
  await page.locator('#navLinkTransactions, #mobileTabTransactions').first().click();
  await page.waitForTimeout(500);

  console.log('5. Navigating to WhatsApp Reminders Module...');
  await page.locator('#navLinkReminders, #mobileTabReminders').first().click();
  await page.waitForTimeout(500);

  console.log('6. Opening Modals (Settings, UPI QR, Add Customer, Add Transaction)...');
  await page.locator('#topbarSettingsBtn').click();
  await page.waitForTimeout(300);
  await page.keyboard.press('Escape');

  await page.evaluate(() => openAddCustomerModal());
  await page.waitForTimeout(300);
  await page.keyboard.press('Escape');

  await browser.close();

  console.log('\n--- DIAGNOSTIC RESULTS ---');
  console.log(`Console Errors: ${consoleErrors.length}`);
  consoleErrors.forEach((e, i) => console.log(`  [${i+1}] ${e}`));

  console.log(`Page Uncaught Errors: ${pageErrors.length}`);
  pageErrors.forEach((e, i) => console.log(`  [${i+1}] ${e}`));

  console.log(`Network Failures: ${networkFailures.length}`);
  networkFailures.forEach((e, i) => console.log(`  [${i+1}] ${e}`));

  if (consoleErrors.length === 0 && pageErrors.length === 0 && networkFailures.length === 0) {
    console.log('\n>>> PERFECT: ZERO ERRORS FOUND! Entire application is clean. <<<');
  } else {
    process.exit(1);
  }
}

checkConsoleAndNetwork().catch(err => {
  console.error(err);
  process.exit(1);
});
