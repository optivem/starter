package com.optivem.eshop.systemtest.latest.acceptance;

import com.optivem.eshop.systemtest.latest.acceptance.base.BaseAcceptanceTest;
import com.optivem.eshop.dsl.channel.ChannelType;
import com.optivem.testing.Channel;
import com.optivem.testing.Isolated;

import org.junit.jupiter.api.TestTemplate;

@Isolated
class PlaceOrderPositiveIsolatedTest extends BaseAcceptanceTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldRecordPlacementTimestamp() {
        scenario
                .given().clock()
                    .withTime("2026-01-15T10:30:00Z")
                .when().placeOrder()
                .then().shouldSucceed()
                .and().clock()
                    .hasTime("2026-01-15T10:30:00Z");
    }
}
