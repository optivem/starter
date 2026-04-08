import { UseCaseContext } from '../../use-case-context';
import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';
import { ThenBrowseCouponsResultStage } from '../then/then-browse-coupons';

export class WhenBrowseCoupons {
  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
    private readonly useCaseContext: UseCaseContext,
  ) {}

  then(): ThenBrowseCouponsResultStage {
    return new ThenBrowseCouponsResultStage(this.app, this.ctx, this.useCaseContext);
  }
}
