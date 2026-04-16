import { Result } from '../../../../common/result.js';
import {
  TaxErrorResponse,
  GetTaxResponse,
  ReturnsTaxRateRequest,
} from '../../../../common/dtos.js';

export interface TaxDriver {
  goToTax(): Promise<Result<void, TaxErrorResponse>>;
  getTaxRate(country: string): Promise<Result<GetTaxResponse, TaxErrorResponse>>;
  returnsTaxRate(request: ReturnsTaxRateRequest): Promise<Result<void, TaxErrorResponse>>;
  close(): Promise<void>;
}
