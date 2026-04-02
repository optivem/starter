package com.optivem.shop.dsl.core.usecase.external.clock.usecases;

import com.optivem.shop.dsl.driver.port.external.clock.ClockDriver;
import com.optivem.shop.dsl.driver.port.external.clock.dtos.GetTimeResponse;
import com.optivem.shop.dsl.core.usecase.external.clock.usecases.base.BaseClockUseCase;
import com.optivem.shop.dsl.core.shared.UseCaseResult;
import com.optivem.shop.dsl.core.shared.UseCaseContext;

public class GetTime extends BaseClockUseCase<GetTimeResponse, GetTimeVerification> {
    public GetTime(ClockDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    @Override
    public UseCaseResult<GetTimeResponse, GetTimeVerification> execute() {
        var result = driver.getTime();
        return new UseCaseResult<>(result, context, GetTimeVerification::new);
    }
}
