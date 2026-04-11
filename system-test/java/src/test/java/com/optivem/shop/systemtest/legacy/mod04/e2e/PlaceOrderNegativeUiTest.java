package com.optivem.shop.systemtest.legacy.mod04.e2e;

import com.optivem.shop.systemtest.legacy.mod04.e2e.base.BaseE2eTest;
import org.junit.jupiter.api.Test;

import java.util.UUID;

import static com.optivem.shop.testkit.common.ResultAssert.assertThatResult;
import static com.optivem.shop.systemtest.commons.constants.Defaults.*;
import static org.assertj.core.api.Assertions.assertThat;

class PlaceOrderNegativeUiTest extends BaseE2eTest {
    @Override
    protected void setShopClient() {
        setUpShopUiClient();
    }

    @Test
    void shouldRejectOrderWithNonIntegerQuantity() {
        var homePage = shopUiClient.openHomePage();
        var newOrderPage = homePage.clickNewOrder();

        newOrderPage.inputSku(SKU + "-" + UUID.randomUUID().toString().substring(0, 8));
        newOrderPage.inputQuantity("invalid-quantity");
        newOrderPage.inputCountry(COUNTRY);
        newOrderPage.clickPlaceOrder();

        var result = newOrderPage.getResult();

        assertThatResult(result).isFailure();
        var error = result.getError();
        assertThat(error.getMessage()).isEqualTo("The request contains one or more validation errors");
        assertThat(error.getFields()).anySatisfy(field -> {
            assertThat(field.getField()).isEqualTo("quantity");
            assertThat(field.getMessage()).isEqualTo("Quantity must be an integer");
        });
    }
}
