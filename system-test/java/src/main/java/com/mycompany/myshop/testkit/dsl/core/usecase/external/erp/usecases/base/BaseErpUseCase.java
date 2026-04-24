package com.mycompany.myshop.testkit.dsl.core.usecase.external.erp.usecases.base;

import com.mycompany.myshop.testkit.driver.port.external.erp.ErpDriver;
import com.mycompany.myshop.testkit.dsl.core.shared.BaseUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;

public abstract class BaseErpUseCase<TResponse, TVerification> extends BaseUseCase<ErpDriver, TResponse, TVerification> {
    protected BaseErpUseCase(ErpDriver driver, UseCaseContext context) {
        super(driver, context);
    }
}
