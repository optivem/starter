import type { AssumeStage } from '../assume-stage.js';

export interface AssumeRunning {
  shouldBeRunning(): Promise<AssumeStage>;
}
