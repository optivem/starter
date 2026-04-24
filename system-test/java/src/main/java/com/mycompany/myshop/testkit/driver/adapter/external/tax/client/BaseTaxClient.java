package com.mycompany.myshop.testkit.driver.adapter.external.tax.client;

import com.mycompany.myshop.testkit.driver.adapter.external.tax.client.dtos.ExtGetCountryResponse;
import com.mycompany.myshop.testkit.driver.adapter.external.tax.client.dtos.error.ExtTaxErrorResponse;
import com.mycompany.myshop.testkit.driver.adapter.shared.client.http.JsonHttpClient;
import com.mycompany.myshop.testkit.common.Closer;
import com.mycompany.myshop.testkit.common.Result;

public abstract class BaseTaxClient implements AutoCloseable {
    private static final String HEALTH_ENDPOINT = "/health";
    private static final String COUNTRIES_ENDPOINT = "/api/countries/";

    protected final JsonHttpClient<ExtTaxErrorResponse> httpClient;

    protected BaseTaxClient(String baseUrl) {
        this.httpClient = new JsonHttpClient<>(baseUrl, ExtTaxErrorResponse.class);
    }

    @Override
    public void close() {
        Closer.close(httpClient);
    }

    public Result<Void, ExtTaxErrorResponse> checkHealth() {
        return httpClient.get(HEALTH_ENDPOINT);
    }

    public Result<ExtGetCountryResponse, ExtTaxErrorResponse> getCountry(String country) {
        return httpClient.get(COUNTRIES_ENDPOINT + country, ExtGetCountryResponse.class);
    }
}
