import { UseCaseContext } from '../../use-case-context';
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
    private readonly useCaseContext: UseCaseContext,
  ) {}

  placeOrder(): WhenPlaceOrder {
    return new WhenPlaceOrder(this.app, this.ctx, this.useCaseContext);
  }

  cancelOrder(): WhenCancelOrder {
    return new WhenCancelOrder(this.app, this.ctx, this.useCaseContext);
  }

  viewOrder(): WhenViewOrder {
    return new WhenViewOrder(this.app, this.ctx, this.useCaseContext);
  }

  publishCoupon(): WhenPublishCoupon {
    return new WhenPublishCoupon(this.app, this.ctx, this.useCaseContext);
  }

  browseCoupons(): WhenBrowseCoupons {
    return new WhenBrowseCoupons(this.app, this.ctx, this.useCaseContext);
  }
}
