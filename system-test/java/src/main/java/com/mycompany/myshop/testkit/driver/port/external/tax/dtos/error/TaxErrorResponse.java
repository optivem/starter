package com.mycompany.myshop.testkit.driver.port.external.tax.dtos.error;

public class TaxErrorResponse {
    private final String message;

    public TaxErrorResponse(String message) {
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
