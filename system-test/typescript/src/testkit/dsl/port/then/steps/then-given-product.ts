import type { ThenGivenClock } from './then-given-clock.js';
import type { ThenGivenCountry } from './then-given-country.js';

export interface ThenGivenProduct {
  hasSku(sku: string): this;
  hasPrice(price: number): this;
  clock(): Promise<ThenGivenClock>;
  product(skuAlias: string): Promise<ThenGivenProduct>;
  country(countryAlias: string): Promise<ThenGivenCountry>;
}
