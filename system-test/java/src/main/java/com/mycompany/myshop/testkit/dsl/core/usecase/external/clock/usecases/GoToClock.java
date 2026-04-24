package com.mycompany.myshop.testkit.dsl.core.usecase.external.clock.usecases;

import com.mycompany.myshop.testkit.driver.port.external.clock.ClockDriver;
import com.mycompany.myshop.testkit.dsl.core.usecase.external.clock.usecases.base.BaseClockUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.shared.VoidVerification;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;

public class GoToClock extends BaseClockUseCase<Void, VoidVerification> {
    public GoToClock(ClockDriver clockDriver, UseCaseContext useCaseContext) {
        super(clockDriver, useCaseContext);
    }

    @Override
    public UseCaseResult<Void, VoidVerification> execute() {
        var result = driver.goToClock();
        return new UseCaseResult<>(result.mapError(e -> SystemError.of(e.getMessage())), context, VoidVerification::new);
    }
}
