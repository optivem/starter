import { Result, success, failure } from '../../../../common/result.js';
import { ClockErrorResponse, GetTimeResponse, ReturnsTimeRequest } from '../../../../common/dtos.js';
import { ClockDriver } from '../../../port/external/clock/clock-driver.js';
import { JsonWireMockClient } from '../../shared/wiremock/wiremock-client.js';

export class ClockStubDriver implements ClockDriver {
  private wireMock: JsonWireMockClient;

  constructor(private baseUrl: string) {
    this.wireMock = new JsonWireMockClient(baseUrl);
  }

  async goToClock(): Promise<Result<void, ClockErrorResponse>> {
    const response = await fetch(`${this.baseUrl}/health`);
    if (response.ok) return success(undefined);
    return failure({ message: `Clock stub not available: ${response.status}` });
  }

  async getTime(): Promise<Result<GetTimeResponse, ClockErrorResponse>> {
    const response = await fetch(`${this.baseUrl}/api/time`);
    if (response.ok) {
      const data = (await response.json()) as GetTimeResponse;
      return success(data);
    }
    return failure({ message: `Failed to get time: ${response.status}` });
  }

  async returnsTime(request: ReturnsTimeRequest): Promise<Result<void, ClockErrorResponse>> {
    await this.wireMock.stubGet('/clock/api/time', { time: request.time });
    return success(undefined);
  }

  async close(): Promise<void> {
    await this.wireMock.removeStubs();
  }
}
