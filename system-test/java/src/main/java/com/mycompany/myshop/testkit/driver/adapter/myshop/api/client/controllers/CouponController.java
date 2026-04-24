package com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.controllers;

import com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.dtos.errors.ProblemDetailResponse;
import com.mycompany.myshop.testkit.driver.adapter.shared.client.http.JsonHttpClient;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.BrowseCouponsResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PublishCouponRequest;
import com.mycompany.myshop.testkit.common.Result;

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
