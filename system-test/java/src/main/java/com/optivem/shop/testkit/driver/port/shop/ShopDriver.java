package com.optivem.shop.testkit.driver.port.shop;

import com.optivem.shop.testkit.driver.port.shared.dtos.ErrorResponse;
import com.optivem.shop.testkit.driver.port.shop.dtos.BrowseCouponsResponse;
import com.optivem.shop.testkit.driver.port.shop.dtos.PlaceOrderRequest;
import com.optivem.shop.testkit.driver.port.shop.dtos.PlaceOrderResponse;
import com.optivem.shop.testkit.driver.port.shop.dtos.PublishCouponRequest;
import com.optivem.shop.testkit.driver.port.shop.dtos.ViewOrderResponse;
import com.optivem.shop.testkit.common.Result;

public interface ShopDriver extends AutoCloseable {
    Result<Void, ErrorResponse> goToShop();

    Result<PlaceOrderResponse, ErrorResponse> placeOrder(PlaceOrderRequest request);

    Result<Void, ErrorResponse> cancelOrder(String orderNumber);

    Result<Void, ErrorResponse> deliverOrder(String orderNumber);

    Result<ViewOrderResponse, ErrorResponse> viewOrder(String orderNumber);

    Result<Void, ErrorResponse> publishCoupon(PublishCouponRequest request);

    Result<BrowseCouponsResponse, ErrorResponse> browseCoupons();
}
