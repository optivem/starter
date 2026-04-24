package com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases;

import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.base.BaseMyShopUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.shared.VoidVerification;

public class DeliverOrder extends BaseMyShopUseCase<Void, VoidVerification> {
    private String orderNumberResultAlias;

    public DeliverOrder(MyShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    public DeliverOrder orderNumber(String orderNumberResultAlias) {
        this.orderNumberResultAlias = orderNumberResultAlias;
        return this;
    }

    @Override
    public UseCaseResult<Void, VoidVerification> execute() {
        var orderNumber = context.getResultValue(orderNumberResultAlias);
        var result = driver.deliverOrder(orderNumber);
        return new UseCaseResult<>(result, context, VoidVerification::new);
    }
}
