package com.mycompany.myshop.testkit.dsl.core.scenario.then.steps;

import com.mycompany.myshop.testkit.dsl.core.shared.ResponseVerification;
import com.mycompany.myshop.testkit.dsl.core.usecase.UseCaseDsl;
import com.mycompany.myshop.testkit.dsl.core.scenario.ExecutionResultContext;
import com.mycompany.myshop.testkit.dsl.port.then.steps.ThenSuccess;

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




