package com.optivem.shop.systemtest.legacy.mod04.e2e;

import com.optivem.shop.systemtest.legacy.mod04.e2e.base.BaseE2eTest;
import com.optivem.shop.testkit.driver.adapter.external.erp.client.dtos.ExtCreateProductRequest;
import com.optivem.shop.testkit.driver.port.shop.dtos.OrderStatus;
import com.optivem.shop.testkit.driver.port.shop.dtos.PlaceOrderRequest;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.util.UUID;

import static com.optivem.shop.testkit.common.ResultAssert.assertThatResult;
import static com.optivem.shop.systemtest.commons.constants.Defaults.*;
import static org.assertj.core.api.Assertions.assertThat;

class PlaceOrderPositiveApiTest extends BaseE2eTest {
    @Override
    protected void setShopClient() {
        setUpShopApiClient();
    }

    @Test
    void shouldPlaceOrderForValidInput() {
        // GivenStage
        var sku = SKU + "-" + UUID.randomUUID().toString().substring(0, 8);
        var createProductRequest = ExtCreateProductRequest.builder()
                .id(sku)
                .title("Test Product")
                .description("Test Description")
                .category("Test Category")
                .brand("Test Brand")
                .price("20.00")
                .build();

        var createProductResult = erpClient.createProduct(createProductRequest);
        assertThatResult(createProductResult).isSuccess();

        // WhenStage
        var placeOrderRequest = PlaceOrderRequest.builder()
                .sku(sku)
                .quantity("5")
                .country(COUNTRY)
                .build();

        var placeOrderResult = shopApiClient.orders().placeOrder(placeOrderRequest);
        assertThatResult(placeOrderResult).isSuccess();

        var orderNumber = placeOrderResult.getValue().getOrderNumber();
        assertThat(orderNumber).startsWith("ORD-");

        // ThenStage
        var viewOrderResult = shopApiClient.orders().viewOrder(orderNumber);
        assertThatResult(viewOrderResult).isSuccess();

        var order = viewOrderResult.getValue();
        assertThat(order.getOrderNumber()).isEqualTo(orderNumber);
        assertThat(order.getSku()).isEqualTo(sku);
        assertThat(order.getQuantity()).isEqualTo(5);
        assertThat(order.getUnitPrice()).isEqualTo(new BigDecimal("20.00"));
        assertThat(order.getTotalPrice()).isGreaterThan(BigDecimal.ZERO);
        assertThat(order.getStatus()).isEqualTo(OrderStatus.PLACED);
    }
}
