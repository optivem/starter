package com.mycompany.myshop.systemtest.legacy.mod11.contract.tax;

import com.mycompany.myshop.systemtest.configuration.ExternalSystemMode;

class TaxRealContractTest extends BaseTaxContractTest {
    @Override
    protected ExternalSystemMode getFixedExternalSystemMode() {
        return ExternalSystemMode.REAL;
    }
}
