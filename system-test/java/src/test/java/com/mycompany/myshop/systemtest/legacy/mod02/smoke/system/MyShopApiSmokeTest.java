package com.mycompany.myshop.systemtest.legacy.mod02.smoke.system;

import com.mycompany.myshop.systemtest.legacy.mod02.base.BaseRawTest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.net.URI;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;

import static org.assertj.core.api.Assertions.assertThat;

class MyShopApiSmokeTest extends BaseRawTest {
    private static final String HEALTH_ENDPOINT = "/health";

    @BeforeEach
    void setUp() {
        setUpMyShopHttpClient();
    }

    @Test
    void shouldBeAbleToGoToMyShop() throws Exception {
        var uri = URI.create(getMyShopApiBaseUrl() + HEALTH_ENDPOINT);
        var request = HttpRequest.newBuilder()
                .uri(uri)
                .GET()
                .build();

        var response = myShopApiHttpClient.send(request, HttpResponse.BodyHandlers.ofString());

        assertThat(response.statusCode()).isEqualTo(200);
    }
}


