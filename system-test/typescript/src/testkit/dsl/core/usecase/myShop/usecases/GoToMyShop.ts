import type { MyShopDriver } from '../../../../../driver/port/myShop/my-shop-driver.js';
import { UseCaseResult } from '../../../shared/use-case-result.js';
import { VoidVerification } from '../../../shared/void-verification.js';
import type { UseCaseContext } from '../../../shared/use-case-context.js';
import { BaseMyShopUseCase } from './base/BaseMyShopUseCase.js';

export class GoToMyShop extends BaseMyShopUseCase<void, VoidVerification> {
  constructor(driver: MyShopDriver, context: UseCaseContext) {
    super(driver, context);
  }

  async execute(): Promise<UseCaseResult<void, VoidVerification>> {
    const result = await this.driver.goToMyShop();

    return new UseCaseResult(
      result,
      this.context,
      (_, ctx) => new VoidVerification(undefined, ctx),
    );
  }
}
