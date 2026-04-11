package com.optivem.shop.testkit.core.usecase.shop.usecases;

import com.optivem.shop.testkit.driver.port.shop.ShopDriver;
import com.optivem.shop.testkit.core.usecase.shop.usecases.base.BaseShopUseCase;
import com.optivem.shop.testkit.core.shared.UseCaseResult;
import com.optivem.shop.testkit.core.shared.UseCaseContext;
import com.optivem.shop.testkit.core.shared.VoidVerification;

public class GoToShop extends BaseShopUseCase<Void, VoidVerification> {
    public GoToShop(ShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    @Override
    public UseCaseResult<Void, VoidVerification> execute() {
        var result = driver.goToShop();
        return new UseCaseResult<>(result, context, VoidVerification::new);
    }
}



