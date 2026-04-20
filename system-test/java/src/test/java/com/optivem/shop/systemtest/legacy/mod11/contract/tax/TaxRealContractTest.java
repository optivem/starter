package com.optivem.shop.systemtest.legacy.mod11.contract.tax;

import com.optivem.shop.systemtest.configuration.ExternalSystemMode;

class TaxRealContractTest extends BaseTaxContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}
