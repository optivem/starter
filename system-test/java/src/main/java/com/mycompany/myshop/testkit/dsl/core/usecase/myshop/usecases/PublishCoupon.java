package com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases;

import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PublishCouponRequest;
import com.mycompany.myshop.testkit.driver.port.myshop.MyShopDriver;
import com.mycompany.myshop.testkit.dsl.core.usecase.myshop.usecases.base.BaseMyShopUseCase;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseResult;
import com.mycompany.myshop.testkit.dsl.core.shared.UseCaseContext;
import com.mycompany.myshop.testkit.dsl.core.shared.VoidVerification;

public class PublishCoupon extends BaseMyShopUseCase<Void, VoidVerification> {
    private String couponCodeParamAlias;
    private String discountRate;
    private String validFrom;
    private String validTo;
    private String usageLimit;

    public PublishCoupon(MyShopDriver driver, UseCaseContext context) {
        super(driver, context);
    }

    public PublishCoupon couponCode(String couponCodeParamAlias) {
        this.couponCodeParamAlias = couponCodeParamAlias;
        return this;
    }

    public PublishCoupon discountRate(String discountRate) {
        this.discountRate = discountRate;
        return this;
    }

    public PublishCoupon validFrom(String validFrom) {
        this.validFrom = validFrom;
        return this;
    }

    public PublishCoupon validTo(String validTo) {
        this.validTo = validTo;
        return this;
    }

    public PublishCoupon usageLimit(String usageLimit) {
        this.usageLimit = usageLimit;
        return this;
    }

    @Override
    public UseCaseResult<Void, VoidVerification> execute() {
        var couponCode = context.getParamValue(couponCodeParamAlias);

        var request = PublishCouponRequest.builder()
                .code(couponCode)
                .discountRate(discountRate)
                .validFrom(validFrom)
                .validTo(validTo)
                .usageLimit(usageLimit)
                .build();

        var result = driver.publishCoupon(request);
        return new UseCaseResult<>(result, context, VoidVerification::new);
    }
}
