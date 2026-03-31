// Generic Result pattern for service layer responses

import type { ApiError } from './error.types';

export type Result<T> =
  | { success: true; data: T }
  | { success: false; error: ApiError };

/**
 * Pattern match on a Result type with handlers for success and error cases
 * Provides a functional alternative to imperative if/else checking
 *
 * @param result - The Result to match against
 * @param handlers - Object containing success and error handlers
 * @returns The return value from whichever handler was executed
 */
export function match<T, R>(
  result: Result<T>,
  handlers: {
    success: (data: T) => R;
    error: (error: ApiError) => R;
  }
): R {
  if (result.success) {
    return handlers.success(result.data);
  } else {
    return handlers.error(result.error);
  }
}
