package com.optivem.eshop.monolith.core.dtos;

import com.optivem.eshop.monolith.core.entities.OrderStatus;
import lombok.Data;

import java.math.BigDecimal;
import java.time.Instant;

@Data
public class ViewOrderDetailsResponse {
    private String orderNumber;
    private Instant orderTimestamp;
    private String sku;
    private int quantity;
    private BigDecimal unitPrice;
    private BigDecimal totalPrice;
    private OrderStatus status;
}
