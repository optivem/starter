package com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases;

import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.base.BaseMyShopUseCase;
import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.BrowseCouponsResponse;

public class BrowseCoupons extends BaseMyShopUseCase<BrowseCouponsResponse, BrowseCouponsVerification> {
    public BrowseCoupons(MyShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    @Override
    public UseCaseResult<BrowseCouponsResponse, BrowseCouponsVerification> execute() {
        var result = driver.browseCoupons();
        return new UseCaseResult<>(result, context, BrowseCouponsVerification::new);
    }
}
