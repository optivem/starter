import { Result, success, failure } from '../../../../../common/result.js';
import type { ErpErrorResponse } from '../../../../port/external/erp/dtos/ErpErrorResponse.js';
import type { ReturnsProductRequest } from '../../../../port/external/erp/dtos/ReturnsProductRequest.js';
import { BaseErpClient } from './BaseErpClient.js';

export class ErpRealClient extends BaseErpClient {
  async createProduct(request: ReturnsProductRequest): Promise<Result<void, ErpErrorResponse>> {
    const response = await fetch(`${this.baseUrl}/api/products`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: request.sku,
        title: 'Test Product',
        description: 'Test Product Description',
        price: request.price,
        category: 'Test',
        brand: 'Test',
      }),
    });
    if (response.ok) return success(undefined);
    return failure({ message: `Failed to create product: ${response.status}` });
  }
}
