import { Result, success } from '../../../../common/result.js';
import { ClockErrorResponse, GetTimeResponse, ReturnsTimeRequest } from '../../../../common/dtos.js';
import { ClockDriver } from '../../../port/external/clock/clock-driver.js';

export class ClockRealDriver implements ClockDriver {
  async goToClock(): Promise<Result<void, ClockErrorResponse>> {
    return success(undefined);
  }

  async getTime(): Promise<Result<GetTimeResponse, ClockErrorResponse>> {
    return success({ time: new Date().toISOString() });
  }

  async returnsTime(_request: ReturnsTimeRequest): Promise<Result<void, ClockErrorResponse>> {
    return success(undefined);
  }

  async close(): Promise<void> {}
}
