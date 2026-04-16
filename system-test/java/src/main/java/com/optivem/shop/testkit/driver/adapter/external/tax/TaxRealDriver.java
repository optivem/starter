package com.optivem.shop.testkit.driver.adapter.external.tax;

import com.optivem.shop.testkit.driver.adapter.external.tax.client.TaxRealClient;
import com.optivem.shop.testkit.driver.port.external.tax.dtos.ReturnsTaxRateRequest;
import com.optivem.shop.testkit.driver.port.shared.dtos.ErrorResponse;
import com.optivem.shop.testkit.common.Result;

public class TaxRealDriver extends BaseTaxDriver<TaxRealClient> {
    public TaxRealDriver(String baseUrl) {
        super(new TaxRealClient(baseUrl));
    }

    @Override
    public Result<Void, ErrorResponse> returnsTaxRate(ReturnsTaxRateRequest request) {
        return Result.success();
    }
}
