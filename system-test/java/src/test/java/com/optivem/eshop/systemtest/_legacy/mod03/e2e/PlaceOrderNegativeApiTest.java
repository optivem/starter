package com.optivem.eshop.systemtest._legacy.mod03.e2e;

import com.optivem.eshop.systemtest._legacy.mod03.e2e.base.BaseE2eTest;
import org.junit.jupiter.api.Test;

import java.net.URI;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;

import static com.optivem.eshop.systemtest.commons.constants.Defaults.*;
import static org.assertj.core.api.Assertions.assertThat;

class PlaceOrderNegativeApiTest extends BaseE2eTest {
    @Override
    protected void setShopDriver() {
        setUpShopHttpClient();
    }

    @Test
    void shouldRejectOrderWithNonExistentSku() throws Exception {
        var placeOrderJson = """
                {
                    "sku": "NON-EXISTENT-SKU-12345",
                    "quantity": "%s"
                }
                """.formatted(QUANTITY);

        var response = shopApiHttpClient.send(
                HttpRequest.newBuilder()
                        .uri(URI.create(getShopApiBaseUrl() + "/api/orders"))
                        .header("Content-Type", "application/json")
                        .POST(HttpRequest.BodyPublishers.ofString(placeOrderJson))
                        .build(),
                HttpResponse.BodyHandlers.ofString());

        assertThat(response.statusCode()).isEqualTo(422);
        var errorBody = httpObjectMapper.readTree(response.body());
        assertThat(errorBody.get("detail").asText()).isEqualTo("The request contains one or more validation errors");
        var errors = errorBody.get("errors");
        assertThat(errors).isNotNull();
        assertThat(errors.isArray()).isTrue();
        boolean found = false;
        for (var error : errors) {
            if (error.get("field").asText().equals("sku") && error.get("message").asText().equals("Product does not exist for SKU: NON-EXISTENT-SKU-12345")) {
                found = true;
                break;
            }
        }
        assertThat(found).isTrue();
    }
}
