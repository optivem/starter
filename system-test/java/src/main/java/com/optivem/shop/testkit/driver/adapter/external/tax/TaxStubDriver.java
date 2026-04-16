package com.optivem.shop.testkit.driver.adapter.external.tax;

import com.optivem.shop.testkit.driver.adapter.external.tax.client.TaxStubClient;
import com.optivem.shop.testkit.driver.adapter.external.tax.client.dtos.ExtGetCountryResponse;
import com.optivem.shop.testkit.driver.port.external.tax.dtos.ReturnsTaxRateRequest;
import com.optivem.shop.testkit.driver.port.shared.dtos.ErrorResponse;
import com.optivem.shop.testkit.common.Converter;
import com.optivem.shop.testkit.common.Result;

public class TaxStubDriver extends BaseTaxDriver<TaxStubClient> {
    public TaxStubDriver(String baseUrl) {
        super(new TaxStubClient(baseUrl));
    }

    @Override
    public void close() {
        client.removeStubs();
        super.close();
    }

    @Override
    public Result<Void, ErrorResponse> returnsTaxRate(ReturnsTaxRateRequest request) {
        var country = request.getCountry();
        var taxRate = Converter.toBigDecimal(request.getTaxRate());

        var response = ExtGetCountryResponse.builder()
                .id(country)
                .taxRate(taxRate)
                .countryName(country)
                .build();

        return client.configureGetCountry(country, response)
                .mapError(ext -> ErrorResponse.builder().message(ext.getMessage()).build());
    }
}
