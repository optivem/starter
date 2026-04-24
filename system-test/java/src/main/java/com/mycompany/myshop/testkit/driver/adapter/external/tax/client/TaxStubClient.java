package com.mycompany.myshop.testkit.driver.adapter.external.tax.client;

import com.mycompany.myshop.testkit.driver.adapter.shared.client.http.HttpStatus;
import com.mycompany.myshop.testkit.driver.adapter.external.tax.client.dtos.ExtGetCountryResponse;
import com.mycompany.myshop.testkit.driver.adapter.external.tax.client.dtos.error.ExtTaxErrorResponse;
import com.mycompany.myshop.testkit.common.Result;
import com.mycompany.myshop.testkit.driver.adapter.shared.client.wiremock.JsonWireMockClient;

public class TaxStubClient extends BaseTaxClient {
    private static final String WIREMOCK_COUNTRIES_ENDPOINT = "/tax/api/countries/";

    private final JsonWireMockClient wireMockClient;

    public TaxStubClient(String baseUrl) {
        super(baseUrl);
        this.wireMockClient = new JsonWireMockClient(baseUrl);
    }

    public Result<Void, ExtTaxErrorResponse> configureGetCountry(String country, ExtGetCountryResponse response) {
        return wireMockClient.stubGet(WIREMOCK_COUNTRIES_ENDPOINT + country, HttpStatus.OK, response)
                .mapError(ExtTaxErrorResponse::new);
    }

    public void removeStubs() {
        wireMockClient.removeStubs();
    }
}
