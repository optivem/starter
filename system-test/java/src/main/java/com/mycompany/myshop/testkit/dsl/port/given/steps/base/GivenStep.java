package com.mycompany.myshop.testkit.dsl.port.given.steps.base;

import com.mycompany.myshop.testkit.dsl.port.given.GivenStage;
import com.mycompany.myshop.testkit.dsl.port.then.ThenStage;
import com.mycompany.myshop.testkit.dsl.port.when.WhenStage;

public interface GivenStep {
    GivenStage and();

    WhenStage when();

    ThenStage then();
}


