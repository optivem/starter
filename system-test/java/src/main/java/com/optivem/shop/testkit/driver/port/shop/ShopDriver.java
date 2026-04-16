package com.optivem.shop.testkit.driver.port.shop;

import com.optivem.shop.testkit.driver.port.shop.dtos.error.SystemError;
import com.optivem.shop.testkit.driver.port.shop.dtos.BrowseCouponsResponse;
import com.optivem.shop.testkit.driver.port.shop.dtos.PlaceOrderRequest;
import com.optivem.shop.testkit.driver.port.shop.dtos.PlaceOrderResponse;
import com.optivem.shop.testkit.driver.port.shop.dtos.PublishCouponRequest;
import com.optivem.shop.testkit.driver.port.shop.dtos.ViewOrderResponse;
import com.optivem.shop.testkit.common.Result;

public interface ShopDriver extends AutoCloseable {
    Result<Void, SystemError> goToShop();

    Result<PlaceOrderResponse, SystemError> placeOrder(PlaceOrderRequest request);

    Result<Void, SystemError> cancelOrder(String orderNumber);

    Result<Void, SystemError> deliverOrder(String orderNumber);

    Result<ViewOrderResponse, SystemError> viewOrder(String orderNumber);

    Result<Void, SystemError> publishCoupon(PublishCouponRequest request);

    Result<BrowseCouponsResponse, SystemError> browseCoupons();
}
