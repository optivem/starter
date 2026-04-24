package com.mycompany.myshop.testkit.dsl.port.when.steps;

import com.mycompany.myshop.testkit.dsl.port.when.steps.base.WhenStep;

public interface WhenCancelOrder extends WhenStep {
    WhenCancelOrder withOrderNumber(String orderNumber);
}
