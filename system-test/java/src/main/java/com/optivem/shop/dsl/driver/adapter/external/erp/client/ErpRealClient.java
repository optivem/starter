package com.optivem.shop.dsl.driver.adapter.external.erp.client;

import com.optivem.shop.dsl.driver.adapter.external.erp.client.dtos.ExtCreateProductRequest;
import com.optivem.shop.dsl.driver.adapter.external.erp.client.dtos.error.ExtErpErrorResponse;
import com.optivem.shop.dsl.common.Result;

public class ErpRealClient extends BaseErpClient {
    private static final String PRODUCTS_ENDPOINT = "/api/products";

    public ErpRealClient(String baseUrl) {
        super(baseUrl);
    }

    public Result<Void, ExtErpErrorResponse> createProduct(ExtCreateProductRequest request) {
        return httpClient.post(PRODUCTS_ENDPOINT, request);
    }
}
