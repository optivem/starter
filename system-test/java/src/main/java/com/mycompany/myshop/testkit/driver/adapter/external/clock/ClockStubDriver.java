package com.mycompany.myshop.testkit.driver.adapter.external.clock;

import com.mycompany.myshop.testkit.driver.port.external.clock.ClockDriver;

import com.mycompany.myshop.testkit.driver.adapter.external.clock.client.ClockStubClient;
import com.mycompany.myshop.testkit.driver.adapter.external.clock.client.dtos.ExtGetTimeResponse;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.GetTimeResponse;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.ReturnsTimeRequest;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.error.ClockErrorResponse;
import com.mycompany.myshop.testkit.common.Closer;
import com.mycompany.myshop.testkit.common.Converter;
import com.mycompany.myshop.testkit.common.Result;

public class ClockStubDriver implements ClockDriver {
    private final ClockStubClient client;

    public ClockStubDriver(String baseUrl) {
        this.client = new ClockStubClient(baseUrl);
    }

    @Override
    public void close() {
        client.removeStubs();
        Closer.close(client);
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
        var time = Converter.toInstant(request.getTime());

        var extResponse = ExtGetTimeResponse.builder()
                .time(time)
                .build();

        return client.configureGetTime(extResponse)
                .mapError(ext -> new ClockErrorResponse(ext.getMessage()));
    }
}
