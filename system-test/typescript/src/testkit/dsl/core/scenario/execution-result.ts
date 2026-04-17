import type { Result } from '../../../common/result.js';
import type { SystemError } from '../../../driver/port/shop/dtos/SystemError.js';
import type { ScenarioContext } from './scenario-context.js';

export interface ExecutionResult<TResponse> {
  result: Result<TResponse, SystemError>;
  context: ScenarioContext;
}
