import type { MyShopDriver } from '../../../../../../driver/port/myShop/my-shop-driver.js';
import { BaseUseCase } from '../../../../shared/base-use-case.js';

export abstract class BaseMyShopUseCase<TResponse, TVerification> extends BaseUseCase<
  MyShopDriver,
  TResponse,
  TVerification
> {}
