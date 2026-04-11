package com.optivem.shop.systemtest.legacy.mod04.smoke.external;

import com.optivem.shop.systemtest.legacy.mod04.base.BaseClientTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static com.optivem.shop.testkit.common.ResultAssert.assertThatResult;

class TaxSmokeTest extends BaseClientTest {
    @BeforeEach
    void setUp() {
        setUpExternalClients();
    }

    @Test
    void shouldBeAbleToGoToTax() {
        var result = taxClient.checkHealth();
        assertThatResult(result).isSuccess();
    }
}
