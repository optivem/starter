package com.mycompany.myshop.testkit.dsl.core.scenario.then.steps;

import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.ErrorVerification;
import com.mycompany.myshop.testkit.dsl.core.shared.ResponseVerification;
import com.mycompany.myshop.testkit.dsl.core.shared.VoidVerification;
import com.mycompany.myshop.testkit.dsl.core.usecase.UseCaseDsl;
import com.mycompany.myshop.testkit.dsl.core.scenario.ExecutionResultContext;
import com.mycompany.myshop.testkit.dsl.port.then.steps.ThenFailure;

public class ThenFailureImpl<TSuccessResponse, TSuccessVerification extends ResponseVerification<TSuccessResponse>>
        extends BaseThenStep<Void, VoidVerification> implements ThenFailure {
    private final ErrorVerification failureVerification;

    public ThenFailureImpl(UseCaseDsl app, ExecutionResultContext executionResult,
            UseCaseResult<TSuccessResponse, TSuccessVerification> result) {
        super(app, executionResult, null);
        if (result == null) {
            throw new IllegalStateException("Cannot verify failure: no operation was executed");
        }
        this.failureVerification = result.shouldFail();
    }

    public ThenFailureImpl<TSuccessResponse, TSuccessVerification> errorMessage(String expectedMessage) {
        failureVerification.errorMessage(expectedMessage);
        return this;
    }

    public ThenFailureImpl<TSuccessResponse, TSuccessVerification> fieldErrorMessage(
            String expectedField, String expectedMessage) {
        failureVerification.fieldErrorMessage(expectedField, expectedMessage);
        return this;
    }

    @Override
    public ThenFailureImpl<TSuccessResponse, TSuccessVerification> and() {
        return this;
    }
}




