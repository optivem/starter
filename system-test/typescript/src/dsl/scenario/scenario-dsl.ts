import { AppContext } from './app-context';
import { ScenarioContext } from './scenario-context';
import { UseCaseContext } from '../use-case-context';
import { AssumeStage } from './assume/assume-stage';
import { GivenStage } from './given/given-stage';
import { WhenStage } from './when/when-stage';

export class ScenarioDsl {
  private readonly useCaseContext: UseCaseContext;

  constructor(private readonly app: AppContext, useCaseContext: UseCaseContext) {
    this.useCaseContext = useCaseContext;
  }

  assume(): AssumeStage {
    return new AssumeStage(this.app);
  }

  given(): GivenStage {
    return new GivenStage(this.app, new ScenarioContext(), this.useCaseContext);
  }

  when(): WhenStage {
    return new WhenStage(this.app, new ScenarioContext(), this.useCaseContext);
  }

  async close(): Promise<void> {
    await this.app.closeAll();
  }
}
