package com.optivem.shop.testkit.port.given.steps;

import com.optivem.shop.testkit.port.given.steps.base.GivenStep;

public interface GivenProduct extends GivenStep {
    GivenProduct withSku(String sku);

    GivenProduct withUnitPrice(String unitPrice);

    GivenProduct withUnitPrice(double unitPrice);
}
