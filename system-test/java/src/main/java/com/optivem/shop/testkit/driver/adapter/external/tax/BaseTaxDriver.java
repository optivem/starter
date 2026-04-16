package com.optivem.shop.testkit.driver.adapter.external.tax;

import com.optivem.shop.testkit.driver.port.external.tax.TaxDriver;

import com.optivem.shop.testkit.driver.adapter.external.tax.client.BaseTaxClient;
import com.optivem.shop.testkit.driver.port.external.tax.dtos.GetTaxResponse;
import com.optivem.shop.testkit.driver.port.shared.dtos.ErrorResponse;
import com.optivem.shop.testkit.common.Closer;
import com.optivem.shop.testkit.common.Result;

public abstract class BaseTaxDriver<TClient extends BaseTaxClient> implements TaxDriver {
    protected final TClient client;

    protected BaseTaxDriver(TClient client) {
        this.client = client;
    }

    @Override
    public void close() {
        Closer.close(client);
    }

    @Override
    public Result<Void, ErrorResponse> goToTax() {
        return client.checkHealth()
                .mapError(ext -> ErrorResponse.builder().message(ext.getMessage()).build());
    }

    @Override
    public Result<GetTaxResponse, ErrorResponse> getTaxRate(String country) {
        return client.getCountry(country)
                .map(taxRateResponse -> GetTaxResponse.builder()
                        .country(taxRateResponse.getId())
                        .taxRate(taxRateResponse.getTaxRate())
                        .build())
                .mapError(ext -> ErrorResponse.builder().message(ext.getMessage()).build());
    }
}
