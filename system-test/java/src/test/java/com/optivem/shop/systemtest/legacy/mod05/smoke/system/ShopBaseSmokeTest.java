package com.optivem.shop.systemtest.legacy.mod05.smoke.system;

import com.optivem.shop.systemtest.legacy.mod05.base.BaseDriverTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static com.optivem.shop.dsl.common.ResultAssert.assertThatResult;

public abstract class ShopBaseSmokeTest extends BaseDriverTest {
    @BeforeEach
    void setUp() {
        setShopDriver();
    }

    protected abstract void setShopDriver();

    @Test
    void shouldBeAbleToGoToShop() {
        var result = shopDriver.goToShop();
        assertThatResult(result).isSuccess();
    }
}

