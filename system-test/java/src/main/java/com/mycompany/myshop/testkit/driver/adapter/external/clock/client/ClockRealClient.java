package com.mycompany.myshop.testkit.driver.adapter.external.clock.client;

import com.mycompany.myshop.testkit.common.Result;
import com.mycompany.myshop.testkit.driver.adapter.external.clock.client.dtos.ExtGetTimeResponse;
import com.mycompany.myshop.testkit.driver.adapter.external.clock.client.dtos.error.ExtClockErrorResponse;

import java.time.Instant;

public class ClockRealClient {
    public Result<Void, ExtClockErrorResponse> checkHealth() {
        now();
        return Result.success();
    }

    public Result<ExtGetTimeResponse, ExtClockErrorResponse> getTime() {
        var response = ExtGetTimeResponse.builder()
                .time(now())
                .build();
        return Result.success(response);
    }

    private static Instant now() {
        return Instant.now();
    }
}
