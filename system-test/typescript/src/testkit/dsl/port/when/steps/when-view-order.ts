import type { ThenResultStage } from '../../then/then-result-stage.js';

export interface WhenViewOrder {
  withOrderNumber(orderNumber: string): this;
  then(): ThenResultStage;
}
