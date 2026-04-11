package com.optivem.shop.testkit.port;

import com.optivem.shop.testkit.port.assume.AssumeStage;
import com.optivem.shop.testkit.port.given.GivenStage;
import com.optivem.shop.testkit.port.when.WhenStage;

public interface ScenarioDsl {
    AssumeStage assume();

    GivenStage given();

    WhenStage when();
}