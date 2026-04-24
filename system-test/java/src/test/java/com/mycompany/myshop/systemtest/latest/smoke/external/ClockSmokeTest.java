package com.mycompany.myshop.systemtest.latest.smoke.external;

import com.mycompany.myshop.systemtest.latest.base.BaseScenarioDslTest;
import org.junit.jupiter.api.Test;

class ClockSmokeTest extends BaseScenarioDslTest {
    @Test
    void shouldBeAbleToGoToClock() {
        scenario.assume().clock().shouldBeRunning();
    }
}


