package com.optivem.eshop.systemtest._legacy.mod08.e2e;

import com.optivem.eshop.dsl.channel.ChannelType;
import com.optivem.eshop.systemtest._legacy.mod08.e2e.base.BaseE2eTest;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

class PlaceOrderNegativeTest extends BaseE2eTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldRejectOrderWithNonExistentSku() {
        scenario
                .when().placeOrder().withSku("NON-EXISTENT-SKU-12345")
                .then().shouldFail()
                .errorMessage("The request contains one or more validation errors")
                .fieldErrorMessage("sku", "Product does not exist for SKU: NON-EXISTENT-SKU-12345");
    }
}
