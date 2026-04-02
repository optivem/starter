package com.optivem.shop.dsl.driver.port.external.erp.dtos;

import lombok.Builder;
import lombok.Data;

@Data
@Builder
public class GetProductRequest {
    private String sku;
}
