package com.mycompany.myshop.systemtest.legacy.mod06.e2e;

import com.mycompany.myshop.testkit.channel.ChannelType;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PlaceOrderRequest;
import com.mycompany.myshop.systemtest.legacy.mod06.e2e.base.BaseE2eTest;
import com.optivem.testing.Channel;
import org.junit.jupiter.api.TestTemplate;

import java.util.UUID;

import static com.mycompany.myshop.testkit.common.ResultAssert.assertThatResult;
import static com.mycompany.myshop.systemtest.commons.constants.Defaults.COUNTRY;
import static com.mycompany.myshop.systemtest.commons.constants.Defaults.SKU;
import static org.assertj.core.api.Assertions.assertThat;

class PlaceOrderNegativeTest extends BaseE2eTest {
    @TestTemplate
    @Channel({ChannelType.UI, ChannelType.API})
    void shouldRejectOrderWithNonIntegerQuantity() {
        var request = PlaceOrderRequest.builder()
                .sku(SKU + "-" + UUID.randomUUID().toString().substring(0, 8))
                .quantity("3.5")
                .country(COUNTRY)
                .build();

        var result = myShopDriver.placeOrder(request);

        assertThatResult(result).isFailure();
        var error = result.getError();
        assertThat(error.getMessage()).isEqualTo("The request contains one or more validation errors");
        assertThat(error.getFields()).anySatisfy(field -> {
            assertThat(field.getField()).isEqualTo("quantity");
            assertThat(field.getMessage()).isEqualTo("Quantity must be an integer");
        });
    }
}
