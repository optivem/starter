import { HealthController } from './controllers/HealthController.js';
import { OrderController } from './controllers/OrderController.js';
import { CouponController } from './controllers/CouponController.js';

export class MyShopApiClient {
  private readonly healthController: HealthController;
  private readonly orderController: OrderController;
  private readonly couponController: CouponController;

  constructor(baseUrl: string) {
    this.healthController = new HealthController(baseUrl);
    this.orderController = new OrderController(baseUrl);
    this.couponController = new CouponController(baseUrl);
  }

  health(): HealthController {
    return this.healthController;
  }

  orders(): OrderController {
    return this.orderController;
  }

  coupons(): CouponController {
    return this.couponController;
  }
}
