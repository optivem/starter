import type { ThenResultStage } from '../../then/then-result-stage.js';

export interface WhenPublishCoupon {
  withCouponCode(couponCode: string): this;
  withDiscountRate(discountRate: string | number): this;
  withValidFrom(validFrom: string): this;
  withValidTo(validTo: string): this;
  withUsageLimit(usageLimit: string | number): this;
  then(): ThenResultStage;
}
