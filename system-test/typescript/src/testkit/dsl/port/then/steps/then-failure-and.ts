import type { ThenOrder } from './then-order.js';
import type { ThenCoupon } from './then-coupon.js';
import type { ThenGivenClock } from './then-given-clock.js';
import type { ThenGivenProduct } from './then-given-product.js';
import type { ThenGivenCountry } from './then-given-country.js';

export interface ThenFailureAnd {
  order(orderNumber?: string): ThenOrder;
  coupon(couponCode?: string): ThenCoupon;
  clock(): Promise<ThenGivenClock>;
  product(skuAlias: string): Promise<ThenGivenProduct>;
  country(countryAlias: string): Promise<ThenGivenCountry>;
}
