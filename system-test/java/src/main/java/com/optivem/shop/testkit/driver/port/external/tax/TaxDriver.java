package com.optivem.shop.testkit.driver.port.external.tax;

import com.optivem.shop.testkit.driver.port.external.tax.dtos.GetTaxResponse;
import com.optivem.shop.testkit.driver.port.external.tax.dtos.ReturnsTaxRateRequest;
import com.optivem.shop.testkit.driver.port.external.tax.dtos.error.TaxErrorResponse;
import com.optivem.shop.testkit.common.Result;

public interface TaxDriver extends AutoCloseable {
    Result<Void, TaxErrorResponse> goToTax();

    Result<GetTaxResponse, TaxErrorResponse> getTaxRate(String country);

    Result<Void, TaxErrorResponse> returnsTaxRate(ReturnsTaxRateRequest request);
}
