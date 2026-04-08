import { AppContext } from './app-context';
import { ScenarioContext } from './scenario-context';
import { AssumeStage } from './assume/assume-stage';
import { GivenStage } from './given/given-stage';
import { WhenStage } from './when/when-stage';

export class ScenarioDsl {
  constructor(private readonly app: AppContext) {}

  assume(): AssumeStage {
    return new AssumeStage(this.app);
  }

  given(): GivenStage {
    return new GivenStage(this.app, new ScenarioContext());
  }

  when(): WhenStage {
    return new WhenStage(this.app, new ScenarioContext());
  }

  async close(): Promise<void> {
    await this.app.closeAll();
  }
}
