package com.optivem.shop.dsl.driver.port.external.clock.dtos;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class ReturnsTimeRequest {
    String time;
}
