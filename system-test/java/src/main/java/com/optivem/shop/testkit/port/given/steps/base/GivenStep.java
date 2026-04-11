package com.optivem.shop.testkit.port.given.steps.base;

import com.optivem.shop.testkit.port.given.GivenStage;
import com.optivem.shop.testkit.port.then.ThenStage;
import com.optivem.shop.testkit.port.when.WhenStage;

public interface GivenStep {
    GivenStage and();

    WhenStage when();

    ThenStage then();
}


