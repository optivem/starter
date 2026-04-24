package com.mycompany.myshop.testkit.dsl.port.given.steps;

import com.mycompany.myshop.testkit.dsl.port.given.steps.base.GivenStep;

public interface GivenPromotion extends GivenStep {
    GivenPromotion withActive(boolean promotionActive);
    GivenPromotion withDiscount(double discount);
    GivenPromotion withDiscount(String discount);
}
