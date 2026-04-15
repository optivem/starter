import type { ThenGivenClock } from './steps/then-given-clock.js';
import type { ThenGivenProduct } from './steps/then-given-product.js';
import type { ThenGivenCountry } from './steps/then-given-country.js';

export interface ThenStage {
  clock(): Promise<ThenGivenClock>;
  product(skuAlias: string): Promise<ThenGivenProduct>;
  country(countryAlias: string): Promise<ThenGivenCountry>;
}
