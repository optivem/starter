import { PromotionConfig } from '../scenario-context';
import { ThenContractStage } from '../then/then-contract';
import { WhenStage } from '../when/when-stage';
import type { GivenStage } from './given-stage';

export class GivenPromotion {
  constructor(
    private readonly stage: GivenStage,
    private readonly config: PromotionConfig,
  ) {}

  withActive(promotionActive: boolean): GivenPromotion {
    this.config.promotionActive = promotionActive;
    return this;
  }

  withDiscount(discount: number | string): GivenPromotion {
    this.config.discount = typeof discount === 'number' ? discount.toFixed(2) : discount;
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
