package com.optivem.shop.systemtest.legacy.mod04.smoke.system;

import com.optivem.shop.systemtest.legacy.mod04.base.BaseClientTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static com.optivem.shop.dsl.common.ResultAssert.assertThatResult;

class ShopApiSmokeTest extends BaseClientTest {
    @BeforeEach
    void setUp() {
        setUpShopApiClient();
    }

    @Test
    void shouldBeAbleToGoToShop() {
        var result = shopApiClient.health().checkHealth();
        assertThatResult(result).isSuccess();
    }
}


