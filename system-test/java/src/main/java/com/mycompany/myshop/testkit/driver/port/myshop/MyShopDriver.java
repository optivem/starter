package com.mycompany.myshop.testkit.driver.port.myshop;

import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.BrowseCouponsResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PlaceOrderRequest;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PlaceOrderResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PublishCouponRequest;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.ViewOrderResponse;
import com.mycompany.myshop.testkit.common.Result;

public interface MyShopDriver extends AutoCloseable {
    Result<Void, SystemError> goToMyShop();

    Result<PlaceOrderResponse, SystemError> placeOrder(PlaceOrderRequest request);

    Result<Void, SystemError> cancelOrder(String orderNumber);

    Result<Void, SystemError> deliverOrder(String orderNumber);

    Result<ViewOrderResponse, SystemError> viewOrder(String orderNumber);

    Result<Void, SystemError> publishCoupon(PublishCouponRequest request);

    Result<BrowseCouponsResponse, SystemError> browseCoupons();
}
