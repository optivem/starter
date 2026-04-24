package com.mycompany.myshop.testkit.driver.adapter.myshop.api;

import com.mycompany.myshop.testkit.driver.port.myshop.dtos.error.SystemError;
import com.mycompany.myshop.testkit.driver.adapter.myshop.api.client.dtos.errors.ProblemDetailResponse;

public class SystemErrorMapper {
    private SystemErrorMapper() {
    }

    public static SystemError from(ProblemDetailResponse problemDetail) {
        var message = problemDetail.getDetail() != null ? problemDetail.getDetail() : "Request failed";

        if (problemDetail.getErrors() != null && !problemDetail.getErrors().isEmpty()) {
            var fieldErrors = problemDetail.getErrors().stream()
                    .map(e -> new SystemError.FieldError(e.getField(), e.getMessage(), e.getCode()))
                    .toList();
            return SystemError.of(message, fieldErrors);
        }

        return SystemError.of(message);
    }
}
