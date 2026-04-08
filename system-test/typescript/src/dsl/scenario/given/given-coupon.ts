import { CouponConfig } from '../scenario-context';
import { WhenStage } from '../when/when-stage';
import type { GivenStage } from './given-stage';

export class GivenCoupon {
  constructor(
    private readonly stage: GivenStage,
    private readonly config: CouponConfig,
  ) {}

  withCode(code: string): GivenCoupon {
    this.config.code = code;
    return this;
  }

  withCouponCode(code: string): GivenCoupon {
    return this.withCode(code);
  }

  withDiscountRate(discountRate: number): GivenCoupon {
    this.config.discountRate = discountRate;
    return this;
  }

  withValidFrom(validFrom: string): GivenCoupon {
    this.config.validFrom = validFrom;
    return this;
  }

  withValidTo(validTo: string): GivenCoupon {
    this.config.validTo = validTo;
    return this;
  }

  withUsageLimit(usageLimit: number | string): GivenCoupon {
    this.config.usageLimit = usageLimit;
    return this;
  }

  and(): GivenStage {
    return this.stage;
  }

  when(): WhenStage {
    return this.stage.when();
  }
}
