import { Result } from '../../../../common/result.js';
import {
  ClockErrorResponse,
  GetTimeResponse,
  ReturnsTimeRequest,
} from '../../../../common/dtos.js';

export interface ClockDriver {
  goToClock(): Promise<Result<void, ClockErrorResponse>>;
  getTime(): Promise<Result<GetTimeResponse, ClockErrorResponse>>;
  returnsTime(request: ReturnsTimeRequest): Promise<Result<void, ClockErrorResponse>>;
  close(): Promise<void>;
}
