package com.mycompany.myshop.testkit.driver.port.external.clock;

import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.GetTimeResponse;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.ReturnsTimeRequest;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.error.ClockErrorResponse;
import com.mycompany.myshop.testkit.common.Result;

public interface ClockDriver extends AutoCloseable {
    Result<Void, ClockErrorResponse> goToClock();
    Result<GetTimeResponse, ClockErrorResponse> getTime();
    Result<Void, ClockErrorResponse> returnsTime(ReturnsTimeRequest request);
}
