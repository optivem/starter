import { useState, useCallback, useRef } from 'react';
import type { ApiError } from '../types/error.types';
import type { Result } from '../types/result.types';
import { match } from '../types/result.types';

/**
 * Custom hook for managing notification state (success messages and errors)
 * Provides a clean API for setting success, error, or clearing both
 * @returns Notification state and control functions
 */
export function useNotification() {
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [error, setError] = useState<ApiError | null>(null);
  const [notificationId, setNotificationId] = useState<number>(0);
  const notificationCounterRef = useRef<number>(0);

  const getNextNotificationId = useCallback(() => {
    notificationCounterRef.current += 1;
    setNotificationId(notificationCounterRef.current);
    return notificationCounterRef.current;
  }, []);

  const clearNotification = useCallback(() => {
    setSuccessMessage(null);
    setError(null);
  }, []);

  const setSuccess = useCallback((message: string) => {
    setSuccessMessage(message);
    setError(null);
    getNextNotificationId();
  }, [getNextNotificationId]);

  const setErrorMessage = useCallback((errorObj: ApiError) => {
    setError(errorObj);
    setSuccessMessage(null);
    getNextNotificationId();
  }, [getNextNotificationId]);

  /**
   * Handles a Result by clearing notifications and auto-handling errors
   * Only requires success handler - error case automatically calls setError
   */
  const handleResult = useCallback(<T>(
    result: Result<T>,
    onSuccess: (data: T) => void
  ) => {
    clearNotification();
    match(result, {
      success: onSuccess,
      error: setErrorMessage
    });
  }, [clearNotification, setErrorMessage]);

  return {
    successMessage,
    error,
    notificationId,
    clearNotification,
    setSuccess,
    setError: setErrorMessage,
    handleResult
  };
}
