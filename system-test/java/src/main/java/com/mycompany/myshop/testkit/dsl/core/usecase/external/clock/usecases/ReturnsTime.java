package com.mycompany.myshop.testkit.dsl.core.usecase.external.clock.usecases;

import com.mycompany.myshop.testkit.driver.port.external.clock.ClockDriver;
import com.mycompany.myshop.testkit.driver.port.external.clock.dtos.ReturnsTimeRequest;
import com.mycompany.myshop.testkit.dsl.core.usecase.external.clock.usecases.base.BaseClockUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.shared.VoidVerification;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;

public class ReturnsTime extends BaseClockUseCase<Void, VoidVerification> {
    private String time;

    public ReturnsTime(ClockDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    public ReturnsTime time(String time) {
        this.time = time;
        return this;
    }

    @Override
    public UseCaseResult<Void, VoidVerification> execute() {
        var request = ReturnsTimeRequest.builder()
                .time(time)
                .build();

        var result = driver.returnsTime(request);

        return new UseCaseResult<>(result.mapError(e -> SystemError.of(e.getMessage())), context, VoidVerification::new);
    }
}
