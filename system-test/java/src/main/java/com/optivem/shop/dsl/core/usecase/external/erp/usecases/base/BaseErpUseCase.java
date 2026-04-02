package com.optivem.shop.dsl.core.usecase.external.erp.usecases.base;

import com.optivem.shop.dsl.driver.port.external.erp.ErpDriver;
import com.optivem.shop.dsl.core.shared.BaseUseCase;
import com.optivem.shop.dsl.core.shared.UseCaseContext;

public abstract class BaseErpUseCase<TResponse, TVerification> extends BaseUseCase<ErpDriver, TResponse, TVerification> {
    protected BaseErpUseCase(ErpDriver driver, UseCaseContext context) {
        super(driver, context);
    }
}
