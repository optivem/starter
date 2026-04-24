package com.mycompany.myshop.systemtest.legacy.mod11.e2e;

import com.mycompany.myshop.testkit.channel.ChannelType;
import com.mycompany.myshop.systemtest.legacy.mod11.e2e.base.BaseE2eTest;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

class PlaceOrderPositiveTest extends BaseE2eTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldPlaceOrder() {
        scenario
                .when().placeOrder()
                .then().shouldSucceed();
    }
}



