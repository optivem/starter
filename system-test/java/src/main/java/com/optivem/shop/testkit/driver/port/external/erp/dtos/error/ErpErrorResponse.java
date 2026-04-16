package com.optivem.shop.testkit.driver.port.external.erp.dtos.error;

public class ErpErrorResponse {
    private final String message;

    public ErpErrorResponse(String message) {
        this.message = message;
    }

    public String getMessage() {
        return message;
    }

    @Override
    public String toString() {
        return message != null ? message : "";
    }
}
