package com.mycompany.myshop.systemtest.legacy.mod11.contract.clock;

import com.mycompany.myshop.systemtest.legacy.mod11.contract.base.BaseExternalSystemContractTest;
import org.junit.jupiter.api.Test;

public abstract class BaseClockContractTest extends BaseExternalSystemContractTest {
    @Test
    void shouldBeAbleToGetTime() {
        scenario
                .given()
                .then().clock().hasTime();
    }
}
