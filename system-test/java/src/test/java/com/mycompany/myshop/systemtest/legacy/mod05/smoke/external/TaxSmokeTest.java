package com.mycompany.myshop.systemtest.legacy.mod05.smoke.external;

import com.mycompany.myshop.systemtest.legacy.mod05.base.BaseDriverTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static com.mycompany.myshop.testkit.common.ResultAssert.assertThatResult;

class TaxSmokeTest extends BaseDriverTest {
    @BeforeEach
    void setUp() {
        setUpExternalDrivers();
    }

    @Test
    void shouldBeAbleToGoToTax() {
        var result = taxDriver.goToTax();
        assertThatResult(result).isSuccess();
    }
}
