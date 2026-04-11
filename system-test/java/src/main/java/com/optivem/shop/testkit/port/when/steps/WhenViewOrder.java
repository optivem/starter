package com.optivem.shop.testkit.port.when.steps;

import com.optivem.shop.testkit.port.when.steps.base.WhenStep;

public interface WhenViewOrder extends WhenStep {
    WhenViewOrder withOrderNumber(String orderNumber);
}

