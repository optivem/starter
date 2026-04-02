package com.optivem.shop.systemtest.legacy.mod07.smoke.external;

import com.optivem.shop.systemtest.legacy.mod07.base.BaseUseCaseDslTest;
import org.junit.jupiter.api.Test;

class ErpSmokeTest extends BaseUseCaseDslTest {
    @Test
    void shouldBeAbleToGoToErp() {
        app.erp().goToErp()
                .execute()
                .shouldSucceed();
    }
}


