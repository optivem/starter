package com.optivem.shop.systemtest.legacy.mod09.smoke.external;

import com.optivem.shop.systemtest.legacy.mod09.base.BaseScenarioDslTest;
import org.junit.jupiter.api.Test;

class ErpSmokeTest extends BaseScenarioDslTest {
    @Test
    void shouldBeAbleToGoToErp() {
        scenario.assume().erp().shouldBeRunning();
    }
}


