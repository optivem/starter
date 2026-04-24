package com.mycompany.myshop.testkit.driver.port.external.erp.dtos;

import lombok.Builder;
import lombok.Data;
import java.math.BigDecimal;

@Data
@Builder
public class GetPromotionResponse {
    private boolean promotionActive;
    private BigDecimal discount;
}
