import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';
import { WhenPlaceOrder } from './when-place-order';
import { WhenCancelOrder } from './when-cancel-order';
import { WhenViewOrder } from './when-view-order';
import { WhenPublishCoupon } from './when-publish-coupon';
import { WhenBrowseCoupons } from './when-browse-coupons';

export class WhenStage {
  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
  ) {}

  placeOrder(): WhenPlaceOrder {
    return new WhenPlaceOrder(this.app, this.ctx);
  }

  cancelOrder(): WhenCancelOrder {
    return new WhenCancelOrder(this.app, this.ctx);
  }

  viewOrder(): WhenViewOrder {
    return new WhenViewOrder(this.app, this.ctx);
  }

  publishCoupon(): WhenPublishCoupon {
    return new WhenPublishCoupon(this.app, this.ctx);
  }

  browseCoupons(): WhenBrowseCoupons {
    return new WhenBrowseCoupons(this.app, this.ctx);
  }
}
