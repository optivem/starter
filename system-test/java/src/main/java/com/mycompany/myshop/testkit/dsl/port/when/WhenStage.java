package com.mycompany.myshop.testkit.dsl.port.when;

import com.mycompany.myshop.testkit.dsl.port.when.steps.WhenBrowseCoupons;
import com.mycompany.myshop.testkit.dsl.port.when.steps.WhenCancelOrder;
import com.mycompany.myshop.testkit.dsl.port.when.steps.WhenPlaceOrder;
import com.mycompany.myshop.testkit.dsl.port.when.steps.WhenPublishCoupon;
import com.mycompany.myshop.testkit.dsl.port.when.steps.WhenViewOrder;

public interface WhenStage {
    WhenPlaceOrder placeOrder();

    WhenCancelOrder cancelOrder();

    WhenViewOrder viewOrder();

    WhenPublishCoupon publishCoupon();

    WhenBrowseCoupons browseCoupons();
}
