package com.optivem.eshop.systemtest.latest.acceptance;

import com.optivem.eshop.systemtest.latest.acceptance.base.BaseAcceptanceTest;
import com.optivem.eshop.dsl.channel.ChannelType;
import com.optivem.testing.Channel;

import org.junit.jupiter.api.TestTemplate;

class PlaceOrderNegativeTest extends BaseAcceptanceTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldRejectOrderWithInvalidQuantity() {
        scenario
                .when().placeOrder()
                    .withQuantity("invalid-quantity")
                .then().shouldFail()
                    .errorMessage("The request contains one or more validation errors")
                    .fieldErrorMessage("quantity", "Quantity must be an integer");
    }

    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldRejectOrderForNonExistentProduct() {
        scenario
                .when().placeOrder()
                    .withSku("NON-EXISTENT-SKU-12345")
                    .withQuantity(1)
                .then().shouldFail()
                    .errorMessage("The request contains one or more validation errors")
                    .fieldErrorMessage("sku", "Product does not exist for SKU: NON-EXISTENT-SKU-12345");
    }

    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldRejectOrderWithEmptySku() {
        scenario
                .when().placeOrder()
                    .withSku("")
                    .withQuantity(1)
                .then().shouldFail()
                    .errorMessage("The request contains one or more validation errors")
                    .fieldErrorMessage("sku", "SKU must not be empty");
    }
}
