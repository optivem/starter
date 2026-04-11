package com.optivem.shop.testkit.driver.port.external.erp;

import com.optivem.shop.testkit.driver.port.external.erp.dtos.GetProductRequest;
import com.optivem.shop.testkit.driver.port.external.erp.dtos.GetProductResponse;
import com.optivem.shop.testkit.driver.port.external.erp.dtos.ReturnsProductRequest;
import com.optivem.shop.testkit.driver.port.external.erp.dtos.ReturnsPromotionRequest;
import com.optivem.shop.testkit.driver.port.shared.dtos.ErrorResponse;
import com.optivem.shop.testkit.common.Result;

public interface ErpDriver extends AutoCloseable {
    Result<Void, ErrorResponse> goToErp();

    Result<GetProductResponse, ErrorResponse> getProduct(GetProductRequest request);

    Result<Void, ErrorResponse> returnsProduct(ReturnsProductRequest request);

    Result<Void, ErrorResponse> returnsPromotion(ReturnsPromotionRequest request);
}
