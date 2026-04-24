package com.mycompany.myshop.systemtest.legacy.mod04.e2e;

import com.mycompany.myshop.systemtest.legacy.mod04.e2e.base.BaseE2eTest;
import com.mycompany.myshop.testkit.driver.port.myshop.dtos.PlaceOrderRequest;
import org.junit.jupiter.api.Test;

import java.util.UUID;

import static com.mycompany.myshop.testkit.common.ResultAssert.assertThatResult;
import static com.mycompany.myshop.systemtest.commons.constants.Defaults.*;
import static org.assertj.core.api.Assertions.assertThat;

class PlaceOrderNegativeApiTest extends BaseE2eTest {
    @Override
    protected void setMyShopClient() {
        setUpMyShopApiClient();
    }

    @Test
    void shouldRejectOrderWithNonIntegerQuantity() {
        var placeOrderRequest = PlaceOrderRequest.builder()
                .sku(SKU + "-" + UUID.randomUUID().toString().substring(0, 8))
                .quantity("invalid-quantity")
                .country(COUNTRY)
                .build();

        var placeOrderResult = myShopApiClient.orders().placeOrder(placeOrderRequest);

        assertThatResult(placeOrderResult).isFailure();
        var error = placeOrderResult.getError();
        assertThat(error.getDetail()).isEqualTo("The request contains one or more validation errors");
        assertThat(error.getErrors()).anySatisfy(field -> {
            assertThat(field.getField()).isEqualTo("quantity");
            assertThat(field.getMessage()).isEqualTo("Quantity must be an integer");
        });
    }
}
