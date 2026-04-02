package com.optivem.shop.dsl.port;

import com.optivem.shop.dsl.port.assume.AssumeStage;
import com.optivem.shop.dsl.port.given.GivenStage;
import com.optivem.shop.dsl.port.when.WhenStage;

public interface ScenarioDsl {
    AssumeStage assume();

    GivenStage given();

    WhenStage when();
}