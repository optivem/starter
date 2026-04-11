package com.optivem.shop.testkit.driver.adapter.shop.api.client.controllers;

import com.optivem.shop.testkit.driver.adapter.shop.api.client.dtos.errors.ProblemDetailResponse;
import com.optivem.shop.testkit.driver.adapter.shared.client.http.JsonHttpClient;
import com.optivem.shop.testkit.driver.port.shop.dtos.BrowseCouponsResponse;
import com.optivem.shop.testkit.driver.port.shop.dtos.PublishCouponRequest;
import com.optivem.shop.testkit.common.Result;

public class CouponController {
    private static final String ENDPOINT = "/api/coupons";

    private final JsonHttpClient<ProblemDetailResponse> httpClient;

    public CouponController(JsonHttpClient<ProblemDetailResponse> httpClient) {
        this.httpClient = httpClient;
    }

    public Result<Void, ProblemDetailResponse> publishCoupon(PublishCouponRequest request) {
        return httpClient.post(ENDPOINT, request);
    }

    public Result<BrowseCouponsResponse, ProblemDetailResponse> browseCoupons() {
        return httpClient.get(ENDPOINT, BrowseCouponsResponse.class);
    }
}
