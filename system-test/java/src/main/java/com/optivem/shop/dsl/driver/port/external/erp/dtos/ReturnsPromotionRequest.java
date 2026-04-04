package com.optivem.shop.dsl.driver.port.external.erp.dtos;

import lombok.Builder;
import lombok.Data;

@Data
@Builder
public class ReturnsPromotionRequest {
    private boolean promotionActive;
    private String discount;
}
