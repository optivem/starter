package com.optivem.eshop.systemtest.legacy.mod03.e2e;

import com.optivem.eshop.systemtest.legacy.mod03.e2e.base.BaseE2eTest;
import org.junit.jupiter.api.Test;

import java.net.URI;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;

import static com.optivem.eshop.systemtest.commons.constants.Defaults.*;
import static org.assertj.core.api.Assertions.assertThat;

class PlaceOrderNegativeApiTest extends BaseE2eTest {
    @Override
    protected void setShopClient() {
        setUpShopHttpClient();
    }

    @Test
    void shouldRejectOrderWithInvalidQuantity() throws Exception {
        var placeOrderJson = """
                {
                    "sku": "%s",
                    "quantity": "invalid-quantity"
                }
                """.formatted(createUniqueSku(SKU));

        var response = shopApiHttpClient.send(
                HttpRequest.newBuilder()
                        .uri(URI.create(getShopApiBaseUrl() + "/api/orders"))
                        .header("Content-Type", "application/json")
                        .POST(HttpRequest.BodyPublishers.ofString(placeOrderJson))
                        .build(),
                HttpResponse.BodyHandlers.ofString());

        assertValidationError(response.statusCode(), response.body(), "quantity", "Quantity must be an integer");
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

        assertValidationError(response.statusCode(), response.body(), "sku", "Product does not exist for SKU: NON-EXISTENT-SKU-12345");
    }

    @Test
    void shouldRejectOrderWithNegativeQuantity() throws Exception {
        var placeOrderJson = """
                {
                    "sku": "%s",
                    "quantity": "-10"
                }
                """.formatted(createUniqueSku(SKU));

        var response = shopApiHttpClient.send(
                HttpRequest.newBuilder()
                        .uri(URI.create(getShopApiBaseUrl() + "/api/orders"))
                        .header("Content-Type", "application/json")
                        .POST(HttpRequest.BodyPublishers.ofString(placeOrderJson))
                        .build(),
                HttpResponse.BodyHandlers.ofString());

        assertValidationError(response.statusCode(), response.body(), "quantity", "Quantity must be positive");
    }

    @Test
    void shouldRejectOrderWithZeroQuantity() throws Exception {
        var placeOrderJson = """
                {
                    "sku": "%s",
                    "quantity": "0"
                }
                """.formatted(createUniqueSku(SKU));

        var response = shopApiHttpClient.send(
                HttpRequest.newBuilder()
                        .uri(URI.create(getShopApiBaseUrl() + "/api/orders"))
                        .header("Content-Type", "application/json")
                        .POST(HttpRequest.BodyPublishers.ofString(placeOrderJson))
                        .build(),
                HttpResponse.BodyHandlers.ofString());

        assertValidationError(response.statusCode(), response.body(), "quantity", "Quantity must be positive");
    }

    @Test
    void shouldRejectOrderWithEmptySku() throws Exception {
        var placeOrderJson = """
                {
                    "sku": "",
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

        assertValidationError(response.statusCode(), response.body(), "sku", "SKU must not be empty");
    }

    @Test
    void shouldRejectOrderWithEmptyQuantity() throws Exception {
        var placeOrderJson = """
                {
                    "sku": "%s",
                    "quantity": ""
                }
                """.formatted(createUniqueSku(SKU));

        var response = shopApiHttpClient.send(
                HttpRequest.newBuilder()
                        .uri(URI.create(getShopApiBaseUrl() + "/api/orders"))
                        .header("Content-Type", "application/json")
                        .POST(HttpRequest.BodyPublishers.ofString(placeOrderJson))
                        .build(),
                HttpResponse.BodyHandlers.ofString());

        assertValidationError(response.statusCode(), response.body(), "quantity", "Quantity must not be empty");
    }

    @Test
    void shouldRejectOrderWithNonIntegerQuantity() throws Exception {
        var placeOrderJson = """
                {
                    "sku": "%s",
                    "quantity": "3.5"
                }
                """.formatted(createUniqueSku(SKU));

        var response = shopApiHttpClient.send(
                HttpRequest.newBuilder()
                        .uri(URI.create(getShopApiBaseUrl() + "/api/orders"))
                        .header("Content-Type", "application/json")
                        .POST(HttpRequest.BodyPublishers.ofString(placeOrderJson))
                        .build(),
                HttpResponse.BodyHandlers.ofString());

        assertValidationError(response.statusCode(), response.body(), "quantity", "Quantity must be an integer");
    }

    @Test
    void shouldRejectOrderWithNullQuantity() throws Exception {
        var placeOrderJson = """
                {
                    "sku": "%s",
                    "quantity": null
                }
                """.formatted(createUniqueSku(SKU));

        var response = shopApiHttpClient.send(
                HttpRequest.newBuilder()
                        .uri(URI.create(getShopApiBaseUrl() + "/api/orders"))
                        .header("Content-Type", "application/json")
                        .POST(HttpRequest.BodyPublishers.ofString(placeOrderJson))
                        .build(),
                HttpResponse.BodyHandlers.ofString());

        assertValidationError(response.statusCode(), response.body(), "quantity", "Quantity must not be empty");
    }

    @Test
    void shouldRejectOrderWithNullSku() throws Exception {
        var placeOrderJson = """
                {
                    "sku": null,
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

        assertValidationError(response.statusCode(), response.body(), "sku", "SKU must not be empty");
    }

    private void assertValidationError(int statusCode, String responseBody, String field, String message) throws Exception {
        assertThat(statusCode).isEqualTo(422);
        var errorBody = httpObjectMapper.readTree(responseBody);
        assertThat(errorBody.get("detail").asText()).isEqualTo("The request contains one or more validation errors");
        var errors = errorBody.get("errors");
        assertThat(errors).isNotNull();
        assertThat(errors.isArray()).isTrue();
        boolean found = false;
        for (var error : errors) {
            if (error.get("field").asText().equals(field) && error.get("message").asText().equals(message)) {
                found = true;
                break;
            }
        }
        assertThat(found).isTrue();
    }
}
