import { Result, success } from '../../../../../common/result.js';
import type { TaxErrorResponse } from '../../../../port/external/tax/dtos/TaxErrorResponse.js';
import type { ReturnsTaxRateRequest } from '../../../../port/external/tax/dtos/ReturnsTaxRateRequest.js';
import { JsonWireMockClient } from '../../../shared/wiremock/wiremock-client.js';
import { BaseTaxClient } from './BaseTaxClient.js';

export class TaxStubClient extends BaseTaxClient {
  private readonly wireMock: JsonWireMockClient;

  constructor(baseUrl: string) {
    super(baseUrl);
    this.wireMock = new JsonWireMockClient(baseUrl);
  }

  async configureTaxRate(request: ReturnsTaxRateRequest): Promise<Result<void, TaxErrorResponse>> {
    await this.wireMock.stubGet(`/tax/api/countries/${request.country}`, {
      id: request.country,
      countryName: request.country,
      taxRate: parseFloat(request.taxRate),
    });
    return success(undefined);
  }

  async close(): Promise<void> {
    await this.wireMock.removeStubs();
  }
}
