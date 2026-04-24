package com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.controllers;

import com.mycompany.myshop.testkit.driver.port.myshop.dtos.ViewOrderResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PlaceOrderRequest;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PlaceOrderResponse;
import com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.dtos.errors.ProblemDetailResponse;
import com.mycompany.myshop.testkit.driver.adapter.shared.client.http.JsonHttpClient;
import com.mycompany.myshop.testkit.common.Result;

public class OrderController {
    private static final String ENDPOINT = "/api/orders";

    private final JsonHttpClient<ProblemDetailResponse> httpClient;

    public OrderController(JsonHttpClient<ProblemDetailResponse> httpClient) {
        this.httpClient = httpClient;
    }

    public Result<PlaceOrderResponse, ProblemDetailResponse> placeOrder(PlaceOrderRequest request) {
        return httpClient.post(ENDPOINT, request, PlaceOrderResponse.class);
    }

    public Result<ViewOrderResponse, ProblemDetailResponse> viewOrder(String orderNumber) {
        return httpClient.get(ENDPOINT + "/" + orderNumber, ViewOrderResponse.class);
    }

    public Result<Void, ProblemDetailResponse> cancelOrder(String orderNumber) {
        return httpClient.post(ENDPOINT + "/" + orderNumber + "/cancel");
    }

    public Result<Void, ProblemDetailResponse> deliverOrder(String orderNumber) {
        return httpClient.post(ENDPOINT + "/" + orderNumber + "/deliver");
    }

}
