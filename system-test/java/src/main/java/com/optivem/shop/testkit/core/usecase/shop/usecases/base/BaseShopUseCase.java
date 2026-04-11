package com.optivem.shop.testkit.core.usecase.shop.usecases.base;

import com.optivem.shop.testkit.driver.port.shop.ShopDriver;
import com.optivem.shop.testkit.core.shared.BaseUseCase;
import com.optivem.shop.testkit.core.shared.UseCaseContext;

public abstract class BaseShopUseCase<TResponse, TVerification> extends BaseUseCase<ShopDriver, TResponse, TVerification> {
    protected BaseShopUseCase(ShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }
}



