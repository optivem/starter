export interface ThenCoupon extends PromiseLike<void> {
  and(): this;
  hasDiscountRate(discountRate: number): this;
  isValidFrom(validFrom: string): this;
  isValidTo(validTo: string): this;
  hasUsageLimit(usageLimit: number): this;
  hasUsedCount(expectedUsedCount: number): this;
}
