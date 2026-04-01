package com.optivem.eshop.systemtest.latest.contract.clock;

import com.optivem.eshop.systemtest.latest.contract.base.BaseExternalSystemContractTest;
import org.junit.jupiter.api.Test;

public abstract class BaseClockContractTest extends BaseExternalSystemContractTest {
    @Test
    void shouldBeAbleToGetTime() {
        scenario
                .given().clock().withTime()
                .then().clock().hasTime();
    }
}
