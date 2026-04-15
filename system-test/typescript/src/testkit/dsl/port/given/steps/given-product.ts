import type { GivenStage } from '../given-stage.js';
import type { WhenStage } from '../../when/when-stage.js';
import type { ThenStage } from '../../then/then-stage.js';

export interface GivenProduct {
  withSku(sku: string): this;
  withUnitPrice(unitPrice: string | number): this;
  and(): GivenStage;
  when(): WhenStage;
  then(): ThenStage;
}
