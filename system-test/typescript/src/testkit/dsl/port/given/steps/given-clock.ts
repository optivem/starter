import type { GivenStage } from '../given-stage.js';
import type { WhenStage } from '../../when/when-stage.js';
import type { ThenStage } from '../../then/then-stage.js';

export interface GivenClock {
  withTime(time?: string): this;
  withWeekday(): this;
  withWeekend(): this;
  and(): GivenStage;
  when(): WhenStage;
  then(): ThenStage;
}
