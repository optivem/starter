package com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases;

import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.base.BaseMyShopUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.shared.VoidVerification;

public class GoToMyShop extends BaseMyShopUseCase<Void, VoidVerification> {
    public GoToMyShop(MyShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    @Override
    public UseCaseResult<Void, VoidVerification> execute() {
        var result = driver.goToMyShop();
        return new UseCaseResult<>(result, context, VoidVerification::new);
    }
}



