package com.mycompany.myshop.testkit.dsl.core.scenario.given.steps;

import com.mycompany.myshop.testkit.dsl.core.usecase.UseCaseDsl;
import com.mycompany.myshop.testkit.dsl.core.scenario.given.GivenImpl;
import com.mycompany.myshop.testkit.dsl.port.then.ThenStage;
import com.mycompany.myshop.testkit.dsl.port.given.steps.base.GivenStep;
import com.mycompany.myshop.testkit.dsl.core.scenario.when.WhenImpl;

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



