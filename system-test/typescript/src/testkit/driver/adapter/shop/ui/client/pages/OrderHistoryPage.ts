import { BasePage, PAGE_TIMEOUT_MS } from './BasePage.js';

export class OrderHistoryPage extends BasePage {
  async fillOrderNumber(orderNumber: string): Promise<void> {
    await this.page.locator("[aria-label='Order Number']").fill(orderNumber, { timeout: PAGE_TIMEOUT_MS });
  }

  async clickSearch(): Promise<void> {
    await this.page.locator("[aria-label='Refresh Order List']").click({ timeout: PAGE_TIMEOUT_MS });
  }

  async clickViewOrderDetails(orderNumber: string): Promise<void> {
    const row = this.page.locator(`//tr[contains(., '${orderNumber}')]`);
    await row.locator("//a[contains(text(), 'View Details')]").click({ timeout: PAGE_TIMEOUT_MS });
  }
}
