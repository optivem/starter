package com.mycompany.myshop.testkit.dsl.core.usecase.external.clock.usecases.base;

import com.mycompany.myshop.testkit.driver.port.external.clock.ClockDriver;
import com.mycompany.myshop.testkit.dsl.core.shared.BaseUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;

public abstract class BaseClockUseCase<TSuccessResponse, TSuccessVerification> extends BaseUseCase<ClockDriver, TSuccessResponse, TSuccessVerification> {
    protected BaseClockUseCase(ClockDriver driver, UseCaseContext context) {
        super(driver, context);
    }
}
