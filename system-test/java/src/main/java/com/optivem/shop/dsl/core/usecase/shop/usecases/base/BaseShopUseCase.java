package com.optivem.shop.dsl.core.usecase.shop.usecases.base;

import com.optivem.shop.dsl.driver.port.shop.ShopDriver;
import com.optivem.shop.dsl.core.shared.BaseUseCase;
import com.optivem.shop.dsl.core.shared.UseCaseContext;

public abstract class BaseShopUseCase<TResponse, TVerification> extends BaseUseCase<ShopDriver, TResponse, TVerification> {
    protected BaseShopUseCase(ShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }
}



