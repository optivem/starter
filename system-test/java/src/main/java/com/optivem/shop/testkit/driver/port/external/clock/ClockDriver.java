package com.optivem.shop.testkit.driver.port.external.clock;

import com.optivem.shop.testkit.driver.port.external.clock.dtos.GetTimeResponse;
import com.optivem.shop.testkit.driver.port.external.clock.dtos.ReturnsTimeRequest;
import com.optivem.shop.testkit.driver.port.external.clock.dtos.error.ClockErrorResponse;
import com.optivem.shop.testkit.common.Result;

public interface ClockDriver extends AutoCloseable {
    Result<Void, ClockErrorResponse> goToClock();
    Result<GetTimeResponse, ClockErrorResponse> getTime();
    Result<Void, ClockErrorResponse> returnsTime(ReturnsTimeRequest request);
}
