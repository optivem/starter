package com.optivem.shop.testkit.port.then.steps.base;

import com.optivem.shop.testkit.port.then.steps.ThenBrowseCoupons;
import com.optivem.shop.testkit.port.then.steps.ThenClock;
import com.optivem.shop.testkit.port.then.steps.ThenCountry;
import com.optivem.shop.testkit.port.then.steps.ThenCoupon;
import com.optivem.shop.testkit.port.then.steps.ThenOrder;
import com.optivem.shop.testkit.port.then.steps.ThenProduct;

public interface ThenStep<TThen> {
    TThen and();

    ThenOrder order();

    ThenOrder order(String orderNumber);

    ThenCoupon coupon();

    ThenCoupon coupon(String couponCode);

    ThenClock clock();

    ThenProduct product(String skuAlias);

    ThenCountry country(String countryAlias);

    ThenBrowseCoupons coupons();
}
