package com.mycompany.myshop.monolith.core.dtos.external;

import lombok.Data;
import java.math.BigDecimal;

@Data
public class GetPromotionResponse {
    private boolean promotionActive;
    private BigDecimal discount;
}
