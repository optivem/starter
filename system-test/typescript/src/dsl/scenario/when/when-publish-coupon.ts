import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';
import { ThenPublishCouponResultStage } from '../then/then-publish-coupon';

export class WhenPublishCoupon {
  private code: string = '';
  private discountRate: number = 0;
  private validFrom?: string;
  private validTo?: string;
  private usageLimit?: number | string;

  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
  ) {}

  withCode(code: string): WhenPublishCoupon {
    this.code = code;
    return this;
  }

  withCouponCode(code: string): WhenPublishCoupon {
    return this.withCode(code);
  }

  withDiscountRate(discountRate: number): WhenPublishCoupon {
    this.discountRate = discountRate;
    return this;
  }

  withValidFrom(validFrom: string): WhenPublishCoupon {
    this.validFrom = validFrom;
    return this;
  }

  withValidTo(validTo: string): WhenPublishCoupon {
    this.validTo = validTo;
    return this;
  }

  withUsageLimit(usageLimit: number | string): WhenPublishCoupon {
    this.usageLimit = usageLimit;
    return this;
  }

  then(): ThenPublishCouponResultStage {
    return new ThenPublishCouponResultStage(this.app, this.ctx, this.code, this.discountRate, this.validFrom, this.validTo, this.usageLimit);
  }
}
