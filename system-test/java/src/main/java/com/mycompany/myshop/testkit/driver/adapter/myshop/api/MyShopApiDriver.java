package com.mycompany.myshop.testkit.driver.adapter.myshop.api;

import com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.MyShopApiClient;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.BrowseCouponsResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PlaceOrderRequest;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PlaceOrderResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PublishCouponRequest;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.ViewOrderResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.common.Closer;
import com.mycompany.myshop.testkit.common.Result;

public class MyShopApiDriver implements MyShopDriver {
    private final MyShopApiClient apiClient;

    public MyShopApiDriver(String baseUrl) {
        this.apiClient = new MyShopApiClient(baseUrl);
    }

    @Override
    public void close() {
        Closer.close(apiClient);
    }

    @Override
    public Result<Void, SystemError> goToMyShop() {
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
