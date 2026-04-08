package com.optivem.shop.systemtest.legacy.mod07.smoke.external;

import com.optivem.shop.systemtest.legacy.mod07.base.BaseUseCaseDslTest;
import org.junit.jupiter.api.Test;

class TaxSmokeTest extends BaseUseCaseDslTest {
    @Test
    void shouldBeAbleToGoToTax() {
        app.tax().goToTax()
                .execute()
                .shouldSucceed();
    }
}
