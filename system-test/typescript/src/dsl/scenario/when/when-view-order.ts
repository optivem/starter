import { DEFAULTS } from '../../defaults';
import { UseCaseContext } from '../../use-case-context';
import { AppContext } from '../app-context';
import { ScenarioContext } from '../scenario-context';
import { ThenViewOrderResultStage } from '../then/then-view-order';

export class WhenViewOrder {
  private orderNumber: string = DEFAULTS.ORDER_NUMBER;

  constructor(
    private readonly app: AppContext,
    private readonly ctx: ScenarioContext,
    private readonly useCaseContext: UseCaseContext,
  ) {}

  withOrderNumber(orderNumber: string): WhenViewOrder {
    this.orderNumber = orderNumber;
    return this;
  }

  then(): ThenViewOrderResultStage {
    return new ThenViewOrderResultStage(this.app, this.ctx, this.useCaseContext, this.orderNumber);
  }
}
