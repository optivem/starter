import type { ThenGivenProduct } from './then-given-product.js';
import type { ThenGivenCountry } from './then-given-country.js';

export interface ThenGivenClock {
  hasTime(time?: string): this;
  clock(): Promise<ThenGivenClock>;
  product(skuAlias: string): Promise<ThenGivenProduct>;
  country(countryAlias: string): Promise<ThenGivenCountry>;
}
