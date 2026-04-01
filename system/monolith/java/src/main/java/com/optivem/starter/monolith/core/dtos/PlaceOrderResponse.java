package com.optivem.starter.monolith.core.dtos;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import lombok.Data;

@Data
@JsonIgnoreProperties(ignoreUnknown = true)
public class PlaceOrderResponse {
    private String orderNumber;
}
