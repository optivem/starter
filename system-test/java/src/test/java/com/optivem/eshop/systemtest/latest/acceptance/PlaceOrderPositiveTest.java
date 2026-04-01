package com.optivem.eshop.systemtest.latest.acceptance;

import com.optivem.eshop.systemtest.latest.acceptance.base.BaseAcceptanceTest;
import com.optivem.eshop.dsl.channel.ChannelType;
import com.optivem.eshop.dsl.driver.port.shop.dtos.OrderStatus;
import com.optivem.testing.Channel;

import org.junit.jupiter.api.TestTemplate;

class PlaceOrderPositiveTest extends BaseAcceptanceTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldPlaceOrderForValidInput() {
        scenario
                .given().product()
                    .withSku("ABC")
                    .withUnitPrice(20.00)
                .when().placeOrder()
                    .withSku("ABC")
                    .withQuantity(5)
                .then().shouldSucceed();
    }

    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldHavePlacedStatusAfterPlacingOrder() {
        scenario
                .when().placeOrder()
                .then().shouldSucceed()
                .and().order()
                    .hasStatus(OrderStatus.PLACED);
    }

    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldHaveORDPrefix() {
        scenario
                .when().placeOrder()
                .then().shouldSucceed()
                .and().order()
                    .hasOrderNumberPrefix("ORD-");
    }
}
