package com.optivem.eshop.systemtest._legacy.mod06.e2e;

import com.optivem.eshop.dsl.channel.ChannelType;
import com.optivem.eshop.dsl.driver.port.shop.dtos.PlaceOrderRequest;
import com.optivem.eshop.systemtest._legacy.mod06.e2e.base.BaseE2eTest;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

import static com.optivem.eshop.dsl.common.ResultAssert.assertThatResult;
import static com.optivem.eshop.systemtest.commons.constants.Defaults.*;
import static org.assertj.core.api.Assertions.assertThat;

class PlaceOrderNegativeTest extends BaseE2eTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldRejectOrderWithNonExistentSku() {
        var request = PlaceOrderRequest.builder()
                .sku("NON-EXISTENT-SKU-12345")
                .quantity(QUANTITY)
                .build();

        var result = shopDriver.placeOrder(request);

        assertThatResult(result).isFailure();
        var error = result.getError();
        assertThat(error.getMessage()).isEqualTo("The request contains one or more validation errors");
        assertThat(error.getFields()).anySatisfy(field -> {
            assertThat(field.getField()).isEqualTo("sku");
            assertThat(field.getMessage()).isEqualTo("Product does not exist for SKU: NON-EXISTENT-SKU-12345");
        });
    }
}
