package com.optivem.shop.testkit.driver.adapter.external.erp.client.dtos;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import java.math.BigDecimal;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class ExtGetPromotionResponse {
    private boolean promotionActive;
    private BigDecimal discount;
}
