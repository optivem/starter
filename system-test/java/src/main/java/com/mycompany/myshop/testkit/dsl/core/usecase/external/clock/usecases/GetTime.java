package com.mycompany.myshop.testkit.dsl.core.usecase.external.clock.usecases;

import com.mycompany.myshop.testkit.driver.port.external.clock.ClockDriver;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.GetTimeResponse;
import com.mycompany.myshop.testkit.dsl.core.usecase.external.clock.usecases.base.BaseClockUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;

public class GetTime extends BaseClockUseCase<GetTimeResponse, GetTimeVerification> {
    public GetTime(ClockDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    @Override
    public UseCaseResult<GetTimeResponse, GetTimeVerification> execute() {
        var result = driver.getTime();
        return new UseCaseResult<>(result.mapError(e -> SystemError.of(e.getMessage())), context, GetTimeVerification::new);
    }
}
