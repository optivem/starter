package com.optivem.shop.dsl.port.given.steps.base;

import com.optivem.shop.dsl.port.given.GivenStage;
import com.optivem.shop.dsl.port.then.ThenStage;
import com.optivem.shop.dsl.port.when.WhenStage;

public interface GivenStep {
    GivenStage and();

    WhenStage when();

    ThenStage then();
}


