package com.mycompany.myshop.systemtest.legacy.mod04.smoke.system;

import com.mycompany.myshop.systemtest.legacy.mod04.base.BaseClientTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static com.mycompany.myshop.testkit.common.ResultAssert.assertThatResult;

class MyShopApiSmokeTest extends BaseClientTest {
    @BeforeEach
    void setUp() {
        setUpMyShopApiClient();
    }

    @Test
    void shouldBeAbleToGoToMyShop() {
        var result = myShopApiClient.health().checkHealth();
        assertThatResult(result).isSuccess();
    }
}


