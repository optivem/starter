package com.mycompany.myshop.systemtest.legacy.mod05.smoke.system;

import com.mycompany.myshop.systemtest.legacy.mod05.base.BaseDriverTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static com.mycompany.myshop.testkit.common.ResultAssert.assertThatResult;

public abstract class MyShopBaseSmokeTest extends BaseDriverTest {
    @BeforeEach
    void setUp() {
        setMyShopDriver();
    }

    protected abstract void setMyShopDriver();

    @Test
    void shouldBeAbleToGoToMyShop() {
        var result = myShopDriver.goToMyShop();
        assertThatResult(result).isSuccess();
    }
}

