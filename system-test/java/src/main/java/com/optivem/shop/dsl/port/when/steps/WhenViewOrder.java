package com.optivem.shop.dsl.port.when.steps;

import com.optivem.shop.dsl.port.when.steps.base.WhenStep;

public interface WhenViewOrder extends WhenStep {
    WhenViewOrder withOrderNumber(String orderNumber);
}

