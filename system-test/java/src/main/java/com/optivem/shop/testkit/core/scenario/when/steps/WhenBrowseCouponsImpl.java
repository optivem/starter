package com.optivem.shop.testkit.core.scenario.when.steps;

import com.optivem.shop.testkit.core.scenario.ExecutionResult;
import com.optivem.shop.testkit.core.scenario.ExecutionResultBuilder;
import com.optivem.shop.testkit.core.usecase.UseCaseDsl;
import com.optivem.shop.testkit.core.usecase.shop.usecases.BrowseCouponsVerification;
import com.optivem.shop.testkit.driver.port.shop.dtos.BrowseCouponsResponse;
import com.optivem.shop.testkit.port.ChannelMode;
import com.optivem.shop.testkit.port.when.steps.WhenBrowseCoupons;

public class WhenBrowseCouponsImpl extends BaseWhenStep<BrowseCouponsResponse, BrowseCouponsVerification> implements WhenBrowseCoupons {

    public WhenBrowseCouponsImpl(UseCaseDsl app) {
        super(app);
    }

    @Override
    protected ExecutionResult<BrowseCouponsResponse, BrowseCouponsVerification> execute(UseCaseDsl app) {
        var result = app.shop(ChannelMode.DYNAMIC).browseCoupons().execute();
        return new ExecutionResultBuilder<>(result).build();
    }
}
