import type { ThenResultStage } from '../../then/then-result-stage.js';

export interface WhenCancelOrder {
  withOrderNumber(orderNumber: string): this;
  then(): ThenResultStage;
}
