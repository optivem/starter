package com.optivem.eshop.monolith.core.dtos.external;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import lombok.Data;

import java.math.BigDecimal;

@Data
@JsonIgnoreProperties(ignoreUnknown = true)
public class ProductDetailsResponse {
    private String id;
    private BigDecimal price;
}
