import type { MyShopDriver } from '../../../../../driver/port/myShop/my-shop-driver.js';
import type { BrowseCouponsResponse } from '../../../../../driver/port/myShop/dtos/BrowseCouponsResponse.js';
import { UseCaseResult } from '../../../shared/use-case-result.js';
import type { UseCaseContext } from '../../../shared/use-case-context.js';
import { BaseMyShopUseCase } from './base/BaseMyShopUseCase.js';
import { BrowseCouponsVerification } from './BrowseCouponsVerification.js';

export class BrowseCoupons extends BaseMyShopUseCase<BrowseCouponsResponse, BrowseCouponsVerification> {
  constructor(driver: MyShopDriver, context: UseCaseContext) {
    super(driver, context);
  }

  async execute(): Promise<UseCaseResult<BrowseCouponsResponse, BrowseCouponsVerification>> {
    const result = await this.driver.browseCoupons();

    return new UseCaseResult(
      result,
      this.context,
      (response, ctx) => new BrowseCouponsVerification(response, ctx),
    );
  }
}
