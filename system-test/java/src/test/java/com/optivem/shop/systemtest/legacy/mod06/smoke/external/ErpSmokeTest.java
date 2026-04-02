package com.optivem.shop.systemtest.legacy.mod06.smoke.external;

import com.optivem.shop.systemtest.legacy.mod06.base.BaseChannelDriverTest;
import org.junit.jupiter.api.Test;

import static com.optivem.shop.dsl.common.ResultAssert.assertThatResult;

class ErpSmokeTest extends BaseChannelDriverTest {
    @Test
    void shouldBeAbleToGoToErp() {
        var result = erpDriver.goToErp();
        assertThatResult(result).isSuccess();
    }
}


