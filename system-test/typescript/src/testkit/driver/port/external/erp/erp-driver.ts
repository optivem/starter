import { Result } from '../../../../common/result.js';
import {
  ErpErrorResponse,
  GetProductResponse,
  ReturnsProductRequest,
  ReturnsPromotionRequest,
} from '../../../../common/dtos.js';

export interface ErpDriver {
  goToErp(): Promise<Result<void, ErpErrorResponse>>;
  getProduct(sku: string): Promise<Result<GetProductResponse, ErpErrorResponse>>;
  returnsProduct(request: ReturnsProductRequest): Promise<Result<void, ErpErrorResponse>>;
  returnsPromotion(request: ReturnsPromotionRequest): Promise<Result<void, ErpErrorResponse>>;
  close(): Promise<void>;
}
