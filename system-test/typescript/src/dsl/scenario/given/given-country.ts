import { CountryConfig } from '../scenario-context';
import { ThenContractStage } from '../then/then-contract';
import { WhenStage } from '../when/when-stage';
import type { GivenStage } from './given-stage';

export class GivenCountry {
  constructor(
    private readonly stage: GivenStage,
    private readonly config: CountryConfig,
  ) {}

  withCode(country: string): GivenCountry {
    this.config.country = country;
    return this;
  }

  withCountry(country: string): GivenCountry {
    return this.withCode(country);
  }

  withTaxRate(taxRate: string | number): GivenCountry {
    this.config.taxRate = typeof taxRate === 'number' ? taxRate.toString() : taxRate;
    return this;
  }

  and(): GivenStage {
    return this.stage;
  }

  when(): WhenStage {
    return this.stage.when();
  }

  then(): ThenContractStage {
    return this.stage.then();
  }
}
