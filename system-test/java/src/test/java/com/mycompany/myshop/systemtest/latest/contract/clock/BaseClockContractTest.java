package com.mycompany.myshop.systemtest.latest.contract.clock;

import com.mycompany.myshop.systemtest.latest.contract.base.BaseExternalSystemContractTest;
import org.junit.jupiter.api.Test;

public abstract class BaseClockContractTest extends BaseExternalSystemContractTest {
    @Test
    void shouldBeAbleToGetTime() {
        scenario
                .given()
                .then().clock().hasTime();
    }
}
