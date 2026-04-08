import { DEFAULTS } from '../../defaults';
import { UseCaseContext } from '../../use-case-context';
import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';
import { ThenCancelOrderResultStage } from '../then/then-cancel-order';

export class WhenCancelOrder {
  private orderNumber: string = DEFAULTS.ORDER_NUMBER;

  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
    private readonly useCaseContext: UseCaseContext,
  ) {}

  withOrderNumber(orderNumber: string): WhenCancelOrder {
    this.orderNumber = orderNumber;
    return this;
  }

  then(): ThenCancelOrderResultStage {
    return new ThenCancelOrderResultStage(this.app, this.ctx, this.useCaseContext, this.orderNumber);
  }
}
