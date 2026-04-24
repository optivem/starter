package com.mycompany.myshop.testkit.dsl.core.usecase.myshop.commons;

import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;
import com.mycompany.myshop.testkit.common.Result;

public final class SystemResults {
    private SystemResults() {
        // Utility class
    }

    public static <T> Result<T, SystemError> success(T value) {
        return Result.success(value);
    }

    public static Result<Void, SystemError> success() {
        return Result.success();
    }

    public static <T> Result<T, SystemError> failure(String message) {
        return Result.failure(SystemError.of(message));
    }

    public static <T> Result<T, SystemError> failure(SystemError error) {
        return Result.failure(error);
    }
}
