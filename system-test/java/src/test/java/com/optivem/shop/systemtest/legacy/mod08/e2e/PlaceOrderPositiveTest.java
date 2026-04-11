package com.optivem.shop.systemtest.legacy.mod08.e2e;

import com.optivem.shop.testkit.channel.ChannelType;
import com.optivem.shop.testkit.driver.port.shop.dtos.OrderStatus;
import com.optivem.shop.systemtest.legacy.mod08.e2e.base.BaseE2eTest;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

class PlaceOrderPositiveTest extends BaseE2eTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldPlaceOrderForValidInput() {
        scenario
                .given().product().withUnitPrice(20.00)
                .when().placeOrder().withQuantity(5)
                .then().shouldSucceed()
                .and().order()
                .hasOrderNumberPrefix("ORD-")
                .hasQuantity(5)
                .hasUnitPrice(20.00)
                .hasStatus(OrderStatus.PLACED)
                .hasTotalPriceGreaterThanZero();
    }
}
