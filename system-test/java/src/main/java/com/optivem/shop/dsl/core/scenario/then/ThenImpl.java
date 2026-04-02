package com.optivem.shop.dsl.core.scenario.then;

import com.optivem.shop.dsl.core.usecase.UseCaseDsl;
import com.optivem.shop.dsl.core.scenario.ExecutionResultContext;
import com.optivem.shop.dsl.core.scenario.then.steps.ThenClockImpl;
import com.optivem.shop.dsl.core.scenario.then.steps.ThenProductImpl;
import com.optivem.shop.dsl.port.then.ThenStage;
import com.optivem.shop.dsl.port.then.steps.ThenClock;
import com.optivem.shop.dsl.port.then.steps.ThenProduct;

public class ThenImpl implements ThenStage {
    protected final UseCaseDsl app;

    public ThenImpl(UseCaseDsl app) {
        this.app = app;
    }

    @Override
    public ThenClock clock() {
        var verification = app.clock().getTime().execute().shouldSucceed();
        return new ThenClockImpl(app, ExecutionResultContext.empty(), verification);
    }

    @Override
    public ThenProduct product(String skuAlias) {
        var verification = app.erp().getProduct().sku(skuAlias).execute().shouldSucceed();
        return new ThenProductImpl(app, ExecutionResultContext.empty(), verification);
    }

}


