package com.mycompany.myshop.testkit.dsl.core.shared;

import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;
import com.mycompany.myshop.testkit.common.Result;

import java.util.function.BiFunction;

import static com.mycompany.myshop.testkit.common.ResultAssert.assertThatResult;

public class UseCaseResult<TSuccessResponse, TSuccessVerification> {
    private final Result<TSuccessResponse, SystemError> result;
    private final UseCaseContext context;
    private final BiFunction<TSuccessResponse, UseCaseContext, TSuccessVerification> successVerificationFactory;

    public UseCaseResult(
            Result<TSuccessResponse, SystemError> result,
            UseCaseContext context,
            BiFunction<TSuccessResponse, UseCaseContext, TSuccessVerification> successVerificationFactory) {
        this.result = result;
        this.context = context;
        this.successVerificationFactory = successVerificationFactory;
    }

    public TSuccessVerification shouldSucceed() {
        assertThatResult(result).isSuccess();
        return successVerificationFactory.apply(result.getValue(), context);
    }

    public ErrorVerification shouldFail() {
        assertThatResult(result).isFailure();
        return new ErrorVerification(result.getError(), context);
    }
}
