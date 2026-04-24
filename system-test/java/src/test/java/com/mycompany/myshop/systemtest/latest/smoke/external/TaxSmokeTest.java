package com.mycompany.myshop.systemtest.latest.smoke.external;

import com.mycompany.myshop.systemtest.latest.base.BaseScenarioDslTest;
import org.junit.jupiter.api.Test;

class TaxSmokeTest extends BaseScenarioDslTest {
    @Test
    void shouldBeAbleToGoToTax() {
        scenario.assume().tax().shouldBeRunning();
    }
}
