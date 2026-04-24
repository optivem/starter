package com.mycompany.myshop.testkit.driver.adapter.external.tax;

import com.mycompany.myshop.testkit.driver.port.external.tax.TaxDriver;

import com.mycompany.myshop.testkit.driver.adapter.external.tax.client.BaseTaxClient;
import com.mycompany.myshop.testkit.driver.port.external.tax.dtos.GetTaxResponse;
import com.mycompany.myshop.testkit.driver.port.external.tax.dtos.error.TaxErrorResponse;
import com.mycompany.myshop.testkit.common.Closer;
import com.mycompany.myshop.testkit.common.Result;

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
    public Result<Void, TaxErrorResponse> goToTax() {
        return client.checkHealth()
                .mapError(ext -> new TaxErrorResponse(ext.getMessage()));
    }

    @Override
    public Result<GetTaxResponse, TaxErrorResponse> getTaxRate(String country) {
        return client.getCountry(country)
                .map(taxRateResponse -> GetTaxResponse.builder()
                        .country(taxRateResponse.getId())
                        .taxRate(taxRateResponse.getTaxRate())
                        .build())
                .mapError(ext -> new TaxErrorResponse(ext.getMessage()));
    }
}
