import { ProductConfig } from '../scenario-context';
import { ThenContractStage } from '../then/then-contract';
import { WhenStage } from '../when/when-stage';
import type { GivenStage } from './given-stage';

export class GivenProduct {
  constructor(
    private readonly stage: GivenStage,
    private readonly config: ProductConfig,
  ) {}

  withSku(sku: string): GivenProduct {
    this.config.sku = sku;
    return this;
  }

  withUnitPrice(price: number | string): GivenProduct {
    this.config.price = typeof price === 'number' ? price.toFixed(2) : price;
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
