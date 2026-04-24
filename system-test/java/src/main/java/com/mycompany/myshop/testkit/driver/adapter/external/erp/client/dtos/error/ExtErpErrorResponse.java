package com.mycompany.myshop.testkit.driver.adapter.external.erp.client.dtos.error;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class ExtErpErrorResponse {
    private String message;
}
