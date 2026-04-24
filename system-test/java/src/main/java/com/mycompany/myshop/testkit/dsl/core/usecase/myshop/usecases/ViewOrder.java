package com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases;

import com.mycompany.myshop.testkit.driver.port.myshop.dtos.ViewOrderResponse;
import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.base.BaseMyShopUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;

public class ViewOrder extends BaseMyShopUseCase<ViewOrderResponse, ViewOrderVerification> {
    private String orderNumberResultAlias;

    public ViewOrder(MyShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    public ViewOrder orderNumber(String orderNumberResultAlias) {
        this.orderNumberResultAlias = orderNumberResultAlias;
        return this;
    }

    @Override
    public UseCaseResult<ViewOrderResponse, ViewOrderVerification> execute() {
        var orderNumber = context.getResultValue(orderNumberResultAlias);

        var result = driver.viewOrder(orderNumber);

        return new UseCaseResult<>(result, context, ViewOrderVerification::new);
    }
}



