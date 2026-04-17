import type { SystemError } from '../../../../port/shop/dtos/SystemError.js';
import type { ProblemDetailResponse } from './dtos/errors/ProblemDetailResponse.js';

export function mapProblemDetail(pd: ProblemDetailResponse): SystemError {
  return {
    message: pd.detail || 'Unknown error',
    fieldErrors: (pd.errors || []).map((e) => ({
      field: e.field,
      message: e.message,
    })),
  };
}
