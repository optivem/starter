import { useState, useEffect, useCallback } from 'react';
import { orderService } from '../services/order-service';
import type { ViewOrderDetailsResponse } from '../types/api.types';

/**
 * Custom hook for managing order details
 * @param orderNumber - The order number to fetch details for
 * @returns Order details state, loading states, and control functions
 */
export function useOrderDetails(orderNumber: string | undefined) {
  const [order, setOrder] = useState<ViewOrderDetailsResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadOrderDetails = useCallback(async () => {
    if (!orderNumber) {
      setError('No order number provided');
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    const result = await orderService.getOrder(orderNumber);

    if (result.success) {
      setOrder(result.data);
      setError(null);
    } else {
      setError(result.error.message);
    }
    setIsLoading(false);
  }, [orderNumber]);

  useEffect(() => {
    loadOrderDetails();
  }, [loadOrderDetails]);

  return {
    order,
    isLoading,
    error,
    refresh: loadOrderDetails
  };
}
