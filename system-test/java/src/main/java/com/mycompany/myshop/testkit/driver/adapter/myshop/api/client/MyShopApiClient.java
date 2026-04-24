package com.mycompany.myshop.testkit.driver.adapter.myshop.api.client;

import com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.controllers.CouponController;
import com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.controllers.HealthController;
import com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.controllers.OrderController;
import com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.dtos.errors.ProblemDetailResponse;
import com.mycompany.myshop.testkit.driver.adapter.shared.client.http.JsonHttpClient;
import com.mycompany.myshop.testkit.common.Closer;

public class MyShopApiClient implements AutoCloseable {
    private final JsonHttpClient<ProblemDetailResponse> httpClient;
    private final HealthController healthController;
    private final OrderController orderController;
    private final CouponController couponController;

    public MyShopApiClient(String baseUrl) {
        this.httpClient = new JsonHttpClient<>(baseUrl, ProblemDetailResponse.class);
        this.healthController = new HealthController(httpClient);
        this.orderController = new OrderController(httpClient);
        this.couponController = new CouponController(httpClient);
    }

    public HealthController health() {
        return healthController;
    }

    public OrderController orders() {
        return orderController;
    }

    public CouponController coupons() {
        return couponController;
    }

    @Override
    public void close() {
        Closer.close(httpClient);
    }
}
