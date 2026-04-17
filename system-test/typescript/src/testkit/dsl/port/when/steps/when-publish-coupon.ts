import type { ThenResultStage } from '../../then/then-result-stage.js';

export interface WhenPublishCoupon {
  withCouponCode(couponCode: string): this;
  withDiscountRate(discountRate: string | number): this;
  withValidFrom(validFrom: string | null): this;
  withValidTo(validTo: string | null): this;
  withUsageLimit(usageLimit: string | number | null): this;
  then(): ThenResultStage;
}
