package com.optivem.shop.testkit.core.scenario.given.steps;

import com.optivem.shop.testkit.core.usecase.UseCaseDsl;
import com.optivem.shop.testkit.core.scenario.given.GivenImpl;
import com.optivem.shop.testkit.port.then.ThenStage;
import com.optivem.shop.testkit.port.given.steps.base.GivenStep;
import com.optivem.shop.testkit.core.scenario.when.WhenImpl;

public abstract class BaseGivenStep implements GivenStep {
    private final GivenImpl given;

    protected BaseGivenStep(GivenImpl given) {
        this.given = given;
    }

    public GivenImpl and() {
        return given;
    }

    public WhenImpl when() {
        return given.when();
    }

    public ThenStage then() {
        return given.then();
    }

    public abstract void execute(UseCaseDsl app);
}



