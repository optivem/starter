package com.optivem.shop.testkit.driver.port.external.clock.dtos.error;

public class ClockErrorResponse {
    private final String message;

    public ClockErrorResponse(String message) {
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
