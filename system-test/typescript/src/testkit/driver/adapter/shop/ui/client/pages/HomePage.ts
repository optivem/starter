import { BasePage, PAGE_TIMEOUT_MS } from './BasePage.js';

export class HomePage extends BasePage {
  async clickNewOrder(): Promise<void> {
    await this.page.locator("a[href='/new-order']").click({ timeout: PAGE_TIMEOUT_MS });
  }

  async clickOrderHistory(): Promise<void> {
    await this.page.locator("a[href='/order-history']").click({ timeout: PAGE_TIMEOUT_MS });
  }

  async clickAdminCoupons(): Promise<void> {
    await this.page.locator("a[href='/admin-coupons']").click({ timeout: PAGE_TIMEOUT_MS });
  }
}
