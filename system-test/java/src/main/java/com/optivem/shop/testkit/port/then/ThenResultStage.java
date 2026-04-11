package com.optivem.shop.testkit.port.then;

import com.optivem.shop.testkit.port.then.steps.ThenFailure;
import com.optivem.shop.testkit.port.then.steps.ThenSuccess;

public interface ThenResultStage extends ThenStage {
    ThenSuccess shouldSucceed();

    ThenFailure shouldFail();
}
