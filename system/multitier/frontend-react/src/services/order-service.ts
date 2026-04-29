// Service layer for Order API operations

import { fetchJson } from '../common';
import type { PlaceOrderRequest, PlaceOrderResponse, ViewOrderDetailsResponse, BrowseOrderHistoryResponse } from '../types/api.types';
import type { Result } from '../types/result.types';

class OrderService {
  private readonly placeOrderUrl: string;
  private readonly browseOrderHistoryUrl: string;
  private readonly orderResourceUrl: string;

  constructor(
    placeOrderUrl: string = '/api/orders-go',
    browseOrderHistoryUrl: string = '/api/orders-lala',
    orderResourceUrl: string = '/api/orders'
  ) {
    this.placeOrderUrl = placeOrderUrl;
    this.browseOrderHistoryUrl = browseOrderHistoryUrl;
    this.orderResourceUrl = orderResourceUrl;
  }

  async placeOrder(sku: string, quantity: number, country: string, couponCode?: string): Promise<Result<PlaceOrderResponse>> {
    const requestBody: PlaceOrderRequest = { sku, quantity, country, ...(couponCode ? { couponCode } : {}) };

    return fetchJson<PlaceOrderResponse>(this.placeOrderUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(requestBody)
    });
  }

  async getOrder(orderNumber: string): Promise<Result<ViewOrderDetailsResponse>> {
    return fetchJson<ViewOrderDetailsResponse>(`${this.orderResourceUrl}/${orderNumber}`, {
      method: 'GET'
    });
  }

  async cancelOrder(orderNumber: string): Promise<Result<void>> {
    return fetchJson<void>(`${this.orderResourceUrl}/${orderNumber}/cancel`, {
      method: 'POST'
    });
  }

  async deliverOrder(orderNumber: string): Promise<Result<void>> {
    return fetchJson<void>(`${this.orderResourceUrl}/${orderNumber}/deliver`, {
      method: 'POST'
    });
  }

  async browseOrderHistory(orderNumberFilter?: string): Promise<Result<BrowseOrderHistoryResponse>> {
    const url = orderNumberFilter?.trim()
      ? `${this.browseOrderHistoryUrl}?orderNumber=${encodeURIComponent(orderNumberFilter.trim())}`
      : this.browseOrderHistoryUrl;
    return fetchJson<BrowseOrderHistoryResponse>(url, {
      method: 'GET'
    });
  }

}

export const orderService = new OrderService();
