import { DEFAULTS } from '../../defaults';
import { ClockConfig } from '../scenario-context';
import { ThenContractStage } from '../then/then-contract';
import { WhenStage } from '../when/when-stage';
import type { GivenStage } from './given-stage';

export class GivenClock {
  constructor(
    private readonly stage: GivenStage,
    private readonly config: ClockConfig,
  ) {}

  withTime(time?: string): GivenClock {
    this.config.time = time || DEFAULTS.CLOCK_TIME;
    return this;
  }

  withWeekday(): GivenClock {
    this.config.time = DEFAULTS.WEEKDAY_TIME;
    return this;
  }

  withWeekend(): GivenClock {
    this.config.time = DEFAULTS.WEEKEND_TIME;
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
