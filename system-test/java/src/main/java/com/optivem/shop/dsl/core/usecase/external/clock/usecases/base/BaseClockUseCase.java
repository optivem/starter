package com.optivem.shop.dsl.core.usecase.external.clock.usecases.base;

import com.optivem.shop.dsl.driver.port.external.clock.ClockDriver;
import com.optivem.shop.dsl.core.shared.BaseUseCase;
import com.optivem.shop.dsl.core.shared.UseCaseContext;

public abstract class BaseClockUseCase<TSuccessResponse, TSuccessVerification> extends BaseUseCase<ClockDriver, TSuccessResponse, TSuccessVerification> {
    protected BaseClockUseCase(ClockDriver driver, UseCaseContext context) {
        super(driver, context);
    }
}
