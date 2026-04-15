import type { GivenStage } from '../given-stage.js';
import type { WhenStage } from '../../when/when-stage.js';
import type { ThenStage } from '../../then/then-stage.js';

export interface GivenOrder {
  withOrderNumber(orderNumber: string): this;
  withSku(sku: string): this;
  withQuantity(quantity: string | number): this;
  withCountry(country: string): this;
  withCouponCode(couponCode: string | null): this;
  withStatus(status: string): this;
  and(): GivenStage;
  when(): WhenStage;
  then(): ThenStage;
}
