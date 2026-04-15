import type { ThenGivenClock } from './then-given-clock.js';
import type { ThenGivenProduct } from './then-given-product.js';

export interface ThenGivenCountry {
  hasCountry(country: string): this;
  hasTaxRate(taxRate: number): this;
  hasTaxRateIsPositive(): this;
  clock(): Promise<ThenGivenClock>;
  product(skuAlias: string): Promise<ThenGivenProduct>;
  country(countryAlias: string): Promise<ThenGivenCountry>;
}
