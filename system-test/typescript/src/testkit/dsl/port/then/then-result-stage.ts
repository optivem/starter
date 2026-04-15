import type { ThenSuccess } from './steps/then-success.js';
import type { ThenFailure } from './steps/then-failure.js';

export interface ThenResultStage {
  shouldSucceed(): ThenSuccess;
  shouldFail(): ThenFailure;
}
