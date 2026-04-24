package com.mycompany.myshop.systemtest.legacy.mod05.smoke.external;

import com.mycompany.myshop.systemtest.legacy.mod05.base.BaseDriverTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static com.mycompany.myshop.testkit.common.ResultAssert.assertThatResult;

class ErpSmokeTest extends BaseDriverTest {
    @BeforeEach
    void setUp() {
        setUpExternalDrivers();
    }

    @Test
    void shouldBeAbleToGoToErp() {
        var result = erpDriver.goToErp();
        assertThatResult(result).isSuccess();
    }
}


