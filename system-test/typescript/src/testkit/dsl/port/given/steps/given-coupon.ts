import type { GivenStage } from '../given-stage.js';
import type { WhenStage } from '../../when/when-stage.js';
import type { ThenStage } from '../../then/then-stage.js';

export interface GivenCoupon {
  withCouponCode(couponCode: string): this;
  withDiscountRate(discountRate: string | number): this;
  withValidFrom(validFrom: string): this;
  withValidTo(validTo: string): this;
  withUsageLimit(usageLimit: string | number): this;
  and(): GivenStage;
  when(): WhenStage;
  then(): ThenStage;
}
