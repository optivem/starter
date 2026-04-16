import { Result } from '../../../common/result.js';
import {
  PlaceOrderRequest,
  PlaceOrderResponse,
  ViewOrderResponse,
  SystemError,
  PublishCouponRequest,
  BrowseCouponsResponse,
} from '../../../common/dtos.js';

export interface ShopDriver {
  goToShop(): Promise<Result<void, SystemError>>;
  placeOrder(request: PlaceOrderRequest): Promise<Result<PlaceOrderResponse, SystemError>>;
  cancelOrder(orderNumber: string): Promise<Result<void, SystemError>>;
  deliverOrder(orderNumber: string): Promise<Result<void, SystemError>>;
  viewOrder(orderNumber: string): Promise<Result<ViewOrderResponse, SystemError>>;
  publishCoupon(request: PublishCouponRequest): Promise<Result<void, SystemError>>;
  browseCoupons(): Promise<Result<BrowseCouponsResponse, SystemError>>;
  close(): Promise<void>;
}
