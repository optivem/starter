package com.optivem.shop.testkit.core.scenario.then.steps;

import com.optivem.shop.testkit.core.shared.ResponseVerification;
import com.optivem.shop.testkit.core.usecase.UseCaseDsl;
import com.optivem.shop.testkit.core.scenario.ExecutionResultContext;
import com.optivem.shop.testkit.port.then.steps.ThenSuccess;

public class ThenSuccessImpl<TSuccessResponse, TSuccessVerification extends ResponseVerification<TSuccessResponse>>
        extends BaseThenStep<TSuccessResponse, TSuccessVerification> implements ThenSuccess {

    public ThenSuccessImpl(UseCaseDsl app, ExecutionResultContext executionResult, TSuccessVerification successVerification) {
        super(app, executionResult, successVerification);
    }

    @Override
    public ThenSuccessImpl<TSuccessResponse, TSuccessVerification> and() {
        return this;
    }
}




