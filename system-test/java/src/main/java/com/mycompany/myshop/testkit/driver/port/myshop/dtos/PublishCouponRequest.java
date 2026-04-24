package com.mycompany.myshop.testkit.driver.port.myshop.dtos;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class PublishCouponRequest {
    private String code;
    private String discountRate;
    private String validFrom;
    private String validTo;
    private String usageLimit;
}
