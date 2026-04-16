package com.optivem.shop.testkit.driver.adapter.shop.api;

import com.optivem.shop.testkit.driver.adapter.shop.api.client.ShopApiClient;
import com.optivem.shop.testkit.driver.port.shop.dtos.error.SystemError;
import com.optivem.shop.testkit.driver.port.shop.dtos.BrowseCouponsResponse;
import com.optivem.shop.testkit.driver.port.shop.dtos.PlaceOrderRequest;
import com.optivem.shop.testkit.driver.port.shop.dtos.PlaceOrderResponse;
import com.optivem.shop.testkit.driver.port.shop.dtos.PublishCouponRequest;
import com.optivem.shop.testkit.driver.port.shop.dtos.ViewOrderResponse;
import com.optivem.shop.testkit.driver.port.shop.ShopDriver;
import com.optivem.shop.testkit.common.Closer;
import com.optivem.shop.testkit.common.Result;

public class ShopApiDriver implements ShopDriver {
    private final ShopApiClient apiClient;

    public ShopApiDriver(String baseUrl) {
        this.apiClient = new ShopApiClient(baseUrl);
    }

    @Override
    public void close() {
        Closer.close(apiClient);
    }

    @Override
    public Result<Void, SystemError> goToShop() {
        return apiClient.health().checkHealth().mapError(SystemErrorMapper::from);
    }

    @Override
    public Result<PlaceOrderResponse, SystemError> placeOrder(PlaceOrderRequest request) {
        return apiClient.orders().placeOrder(request).mapError(SystemErrorMapper::from);
    }

    @Override
    public Result<Void, SystemError> cancelOrder(String orderNumber) {
        return apiClient.orders().cancelOrder(orderNumber).mapError(SystemErrorMapper::from);
    }

    @Override
    public Result<Void, SystemError> deliverOrder(String orderNumber) {
        return apiClient.orders().deliverOrder(orderNumber).mapError(SystemErrorMapper::from);
    }

    @Override
    public Result<ViewOrderResponse, SystemError> viewOrder(String orderNumber) {
        return apiClient.orders().viewOrder(orderNumber).mapError(SystemErrorMapper::from);
    }

    @Override
    public Result<Void, SystemError> publishCoupon(PublishCouponRequest request) {
        return apiClient.coupons().publishCoupon(request).mapError(SystemErrorMapper::from);
    }

    @Override
    public Result<BrowseCouponsResponse, SystemError> browseCoupons() {
        return apiClient.coupons().browseCoupons().mapError(SystemErrorMapper::from);
    }
}
