package com.mycompany.myshop.systemtest.legacy.mod04.smoke.system;

import com.mycompany.myshop.systemtest.legacy.mod04.base.BaseClientTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertTrue;

class MyShopUiSmokeTest extends BaseClientTest {
    @BeforeEach
    void setUp() {
        setUpMyShopUiClient();
    }

    @Test
    void shouldBeAbleToGoToMyShop() {
        myShopUiClient.openHomePage();
        assertTrue(myShopUiClient.isPageLoaded());
    }
}


