package com.optivem.eshop.systemtest.latest.smoke.external;

import com.optivem.eshop.systemtest.latest.base.BaseScenarioDslTest;
import org.junit.jupiter.api.Test;

class ErpSmokeTest extends BaseScenarioDslTest {
    @Test
    void shouldBeAbleToGoToErp() {
        scenario.assume().erp().shouldBeRunning();
    }
}


