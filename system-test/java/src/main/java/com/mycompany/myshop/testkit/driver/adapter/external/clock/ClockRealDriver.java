package com.mycompany.myshop.testkit.driver.adapter.external.clock;

import com.mycompany.myshop.testkit.driver.port.external.clock.ClockDriver;

import com.mycompany.myshop.testkit.driver.adapter.external.clock.client.ClockRealClient;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.GetTimeResponse;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.ReturnsTimeRequest;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.error.ClockErrorResponse;
import com.mycompany.myshop.testkit.common.Result;

public class ClockRealDriver implements ClockDriver {
    private final ClockRealClient client;

    public ClockRealDriver() {
        this.client = new ClockRealClient();
    }

    @Override
    public void close() {
        // No resources to close - ClockRealClient has no HTTP or other resources
    }

    @Override
    public Result<Void, ClockErrorResponse> goToClock() {
        return client.checkHealth()
                .mapError(ext -> new ClockErrorResponse(ext.getMessage()));
    }

    @Override
    public Result<GetTimeResponse, ClockErrorResponse> getTime() {
        return client.getTime()
                .map(ext -> GetTimeResponse.builder().time(ext.getTime()).build())
                .mapError(ext -> new ClockErrorResponse(ext.getMessage()));
    }

    @Override
    public Result<Void, ClockErrorResponse> returnsTime(ReturnsTimeRequest request) {
        // No-op because real clock cannot be configured
        return Result.success();
    }
}
