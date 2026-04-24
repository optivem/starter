package com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.base;

import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.dsl.core.shared.BaseUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;

public abstract class BaseMyShopUseCase<TResponse, TVerification> extends BaseUseCase<MyShopDriver, TResponse, TVerification> {
    protected BaseMyShopUseCase(MyShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }
}



