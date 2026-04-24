package com.mycompany.myshop.systemtest.legacy.mod08.smoke.external;

import com.mycompany.myshop.systemtest.legacy.mod08.base.BaseScenarioDslTest;
import org.junit.jupiter.api.Test;

class TaxSmokeTest extends BaseScenarioDslTest {
    @Test
    void shouldBeAbleToGoToTax() {
        scenario.assume().tax().shouldBeRunning();
    }
}
